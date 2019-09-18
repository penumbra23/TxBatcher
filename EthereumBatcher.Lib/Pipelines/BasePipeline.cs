using EthereumBatcher.Lib.GasEstimation;
using EthereumBatcher.Lib.Helpers;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using StackExchange.Redis;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace EthereumBatcher.Lib.Pipelines
{
    public class BasePipeline
    {
        public const string AddressPrefix = "txProcAddrNoCnt";

        public BasePipeline(RedisWrapper redisWrapper, Web3 web3)
        {
            RedisClient = redisWrapper;
            GasEstimator = new RangeEstimator(web3, 1000000000, 15000000000);
            Web3 = web3;
        }

        public static TimeSpan NonceExpiration { get; set; } = TimeSpan.FromMinutes(5);

        public IEstimator GasEstimator { get; set; }

        protected RedisWrapper RedisClient { get; set; }

        public Web3 Web3 { get; protected set; }

        public async Task<BigInteger> GetNonce(string address)
        {
            HexBigInteger nodeNonce = await Web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address);

            // The final nonce
            BigInteger finalNonce = nodeNonce.Value;

            // Get the key and add the nonce if it doesn't exist
            string nonceKey = $"{AddressPrefix}{address}";

            bool success;
            do
            {
                // Get the current nonce from redis
                string redisNonce = RedisClient.Get(nonceKey); // redisDb.StringGet(nonceKey);

                // If the nonce doesn't exist in the database
                if (redisNonce == null || nodeNonce.Value > BigInteger.Parse(redisNonce))
                {
                    // Set the nonce in redis to the next one after the node nonce
                    ITransaction redisTransaction = RedisClient.CreateTransaction();
                    redisTransaction.AddCondition(Condition.StringEqual(nonceKey, redisNonce));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    redisTransaction.StringSetAsync(nonceKey, (nodeNonce.Value + 1).ToString(), NonceExpiration);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    finalNonce = nodeNonce;
                    success = await redisTransaction.ExecuteAsync();
                }
                else
                {
                    // The final nonce is set to the one retrieved from the ethereum node
                    // If the nodeNonce is lower than the one from redis, use the redis nonce as the final
                    // and increment the nonce in redis      

                    BigInteger redisNonceBigInt = BigInteger.Parse(redisNonce);
                    ITransaction redisTransaction = RedisClient.CreateTransaction();
                    redisTransaction.AddCondition(Condition.StringEqual(nonceKey, redisNonce));
                    finalNonce = redisNonceBigInt;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    redisTransaction.StringIncrementAsync(nonceKey);
                    redisTransaction.KeyExpireAsync(nonceKey, NonceExpiration);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    success = await redisTransaction.ExecuteAsync();
                }
            } while (!success); // TODO: add max number of retries

            return finalNonce;
        }
    }
}
