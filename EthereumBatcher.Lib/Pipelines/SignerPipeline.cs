using EthereumBatcher.Lib.Exceptions;
using EthereumBatcher.Lib.Helpers;
using EthereumBatcher.Lib.Signers;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Threading.Tasks;

namespace EthereumBatcher.Lib.Pipelines
{
    public class SignerPipeline : BasePipeline
    {
        public SignerPipeline(RedisWrapper redis, Web3 web3) : base(redis, web3) { }

        public async Task<string> CallFunction(Function function, HexBigInteger value, HexBigInteger gasLimit = null, params object[] parameters)
        {
            // Check if private key is supplied in web3 instance
            if (string.IsNullOrEmpty(Web3.TransactionManager.Account.Address))
                throw new AuthenticationException("No private key provided in the underlying Web3 instance.");

            // Calls the function to see if it can be executed
            HexBigInteger gas = gasLimit ?? await EstimateGas(new CallInput()
            {
                From = Web3.TransactionManager.Account.Address,
                Data = function.GetData(parameters),
                To = function.ContractAddress,
                Value = value ?? new HexBigInteger(0)
            }).ConfigureAwait(false);

            // Estimate gas price
            BigInteger gasPrice = await GasEstimator.EstimateGasPrice();

            HexBigInteger balance = await Web3.Eth.GetBalance.SendRequestAsync(Web3.TransactionManager.Account.Address);

            if (gasPrice * gas.Value >= (balance.Value * 19 / 20))
                throw new BalanceException("Insufficient balance for gas.");

            var nonce = await GetNonce(Web3.TransactionManager.Account.Address);

            return await function.SendTransactionAsync(new TransactionInput()
            {
                GasPrice = new HexBigInteger(gasPrice),
                Gas = new HexBigInteger(gas),
                From = Web3.TransactionManager.Account.Address,
                To = function.ContractAddress,
                Value = value,
                Nonce = new HexBigInteger(nonce),
                Data = function.GetData(parameters)
            }, parameters);
        }

        public async Task<string> CallFunction(Function function, HexBigInteger value, string privateKey, HexBigInteger gasLimit = null, params object[] parameters)
        {
            // Create the account for retrieving from address
            Account senderAccount = new Account(privateKey);

            // Calls the function to see if it can be executed
            HexBigInteger gas = gasLimit ?? await EstimateGas(new CallInput()
            {
                From = senderAccount.Address,
                Data = function.GetData(parameters),
                To = function.ContractAddress,
                Value = value ?? new HexBigInteger(0)
            }).ConfigureAwait(false);

            // Estimate gas price
            BigInteger gasPrice = await GasEstimator.EstimateGasPrice();

            HexBigInteger balance = await Web3.Eth.GetBalance.SendRequestAsync(Web3.TransactionManager.Account.Address);

            if (gasPrice * gas.Value >= (balance.Value * 19 / 20))
                throw new BalanceException("Insufficient balance for gas.");

            BigInteger nonce = await GetNonce(senderAccount.Address);

            string signedTx = await new EthereumTxSigner(privateKey)
                .Sign(new TransactionInput()
                {
                    GasPrice = new HexBigInteger(gasPrice),
                    Gas = new HexBigInteger(gas),
                    From = senderAccount.Address,
                    To = function.ContractAddress,
                    Value = value,
                    Nonce = new HexBigInteger(nonce),
                    Data = function.GetData(parameters)
                });

            return await Web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTx);
        }

        public async Task<TransactionReceipt> CallFunctionReceipt(Function function, HexBigInteger value, HexBigInteger gasLimit = null, params object[] parameters)
        {
            // Calls the function to see if it can be executed
            // Check if private key is supplied in web3 instance
            if (string.IsNullOrEmpty(Web3.TransactionManager.Account.Address))
                throw new AuthenticationException("No private key provided in the underlying Web3 instance.");

            // Calls the function to see if it can be executed
            HexBigInteger gas = gasLimit ?? await EstimateGas(new CallInput()
            {
                From = Web3.TransactionManager.Account.Address,
                Data = function.GetData(parameters),
                To = function.ContractAddress,
                Value = value ?? new HexBigInteger(0)
            }).ConfigureAwait(false);

            // Estimate gas price
            BigInteger gasPrice = await GasEstimator.EstimateGasPrice();

            HexBigInteger balance = await Web3.Eth.GetBalance.SendRequestAsync(Web3.TransactionManager.Account.Address);

            if (gasPrice * gas.Value >= (balance.Value * 19 / 20))
                throw new BalanceException("Insufficient balance for gas.");

            BigInteger nonce = await GetNonce(Web3.TransactionManager.Account.Address);

            return await function.SendTransactionAndWaitForReceiptAsync(new TransactionInput()
            {
                GasPrice = new HexBigInteger(gasPrice),
                Gas = gas,
                From = Web3.TransactionManager.Account.Address,
                To = function.ContractAddress,
                Value = value,
                Nonce = new HexBigInteger(nonce),
                Data = function.GetData(parameters)
            }, null, parameters);
        }

        private async Task<HexBigInteger> EstimateGas(CallInput input) => await Web3.Eth.Transactions.EstimateGas.SendRequestAsync(input);
    }
}
