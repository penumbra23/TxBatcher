# Ethereum TxBatcher
TxBatcher focuses on surpassing the physical limit of Ethereum (or at least tries the same). Ethereum's throughput is currently peaked around 30 TPS. Some of the reasons why it's being slow implies validating transfers, simulating transaction calls and in general PoW algorithm.

**What is TxBatcher?** It's an extensions of the popular [Nethereum](https://github.com/Nethereum/Nethereum) .NET library for interaction between your C# app and the Ethereum blockchain. 

**What is TxBatcher capable of?** It's capable of sending multiple transactions at the same time from the same Ethereum address. Some TGE and ICO token drops were successfully finished in less than 1 day, with a volume of 20k transactions thanks to this library.

## How does it work?
**TxBatcher** utilizes Redis for storing the latest account nonce. If the transaction is simulated successfull on the local node, the nonce gets written to the Redis storage. Afterwards, each succeeding transaction uses the nonce from the cache and increments it. 

## Usage

**TxBatcher**  test project has an example of using the pipelines for signing:
```cs
// Create your classic web3 instance
Web3 web3 = new Web3(...);

SignerPipeline pipeline = new SignerPipeline(new RedisWrapper("redis:1174"), web3);

// Fetch the contract and function from your ABI
Contract contract = web3.Eth.GetContract(...);
Function func = contract.GetFunction(...);

// Call the function
string hash = await pipeline.CallFunction(func, 0, privateKey, gasLimit, args);

// Call it in a loop
while(i < 200)
{
  await pipeline.CallFunction(func, 0, privateKey, gasLimit, args);
  ++i;
}
```
_Pipelines_ are a way of customizing the transaction signing request. The `SignerPipeline` is the built-in offline signing pipeline. One can create a different pipeline by inheriting `BasePipeline` to get the common functions used for the nonce.

This package offers a `SafeRpcClient` class for invoking RPC calls multiple times with a fallback. If the RPC call fails `CallThreshold` times, an exception gets raised.

The package offers also a `GasEstimation` namespace for estimating the required gas. The library has a built-in `RangeEstimator` and `GasStationEstimator` which takes the best price from [https://ethgasstation.info/](https://ethgasstation.info/) (feel free to implement your own and submit a pull request).

## Notes
The nonce is kept in the cache for 5 minutes by default. Override it by setting `BasePipeline.NonceExpiration`. If the nonce expires, the pipeline takes the current nonce on the blockchain. 

**TAKE CARE** when exceptions get raised! If the cache has the same nonce for too long, the pipeline will take the nonce from the cache which might be too high at that moment. In that case, you need to clear the Redis cache OR adjust the `NonceExpiration` to a lower value.

## License
MIT
