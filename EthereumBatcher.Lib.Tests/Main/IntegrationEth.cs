using EthereumBatcher.Lib.Client;
using EthereumBatcher.Lib.Helpers;
using EthereumBatcher.Lib.Pipelines;
using Nethereum.Contracts;
using Nethereum.Web3;
using System;
using System.Diagnostics;
using Xunit;

namespace EthereumBatcher.Lib.IntegrationTests.Main
{
    public class MainTests
    {
        // Some contract I wrote long ago
        // Just has one method increment that checks if the param is lower than 2 and
        // increments a local counter
        const string contractAbi = "[{'constant': false, 'inputs': [{'name': 'param1', 'type': 'uint256'}],'name': 'increment','outputs': [],'payable': false,	'stateMutability': 'nonpayable','type': 'function'},{'anonymous': false,'inputs': [{'indexed': false,'name': '','type': 'uint256'}],'name': 'Incremented','type': 'event'}]";
        const string contractAddress = "0xf943840287be969ca59931a5b83c0687292c29f9";
        
        // Params required for the TxBatcher
        const string privateKey = "YOUR PRIVATE KEY";
        const string ropstenUrl = "YOU ROPSTEN URL";                    // get one on Infura free
        const string redisConn = "YOUR REDIS CONNECTION STRING";        // respect format: url:port,password=superPass1234
        const int txNumber = 50;

        [Fact]
        public async void BatchMultipleTx()
        {
            string privKey = privateKey;
            Web3 web3 = new Web3(new SafeRpcClient(new Uri(ropstenUrl)));
            SignerPipeline pipeline = new SignerPipeline(new RedisWrapper(redisConn), web3);

            Contract contract = web3.Eth.GetContract(contractAbi,contractAddress);
            Function func = contract.GetFunction("increment");

            for (int i = 0; i < txNumber; ++i)
            {
                string txHash = await pipeline.CallFunction(func, null, privKey, null, 15 + i);
                Trace.WriteLine("TRANSACTION HASH: " + txHash);
                Assert.NotNull(txHash);
            }
        }
    }
}
