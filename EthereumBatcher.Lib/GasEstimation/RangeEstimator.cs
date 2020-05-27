using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace EthereumBatcher.Lib.GasEstimation
{
    /// <summary>
    /// Gets gas estimation from node and clamps between lower and upper.
    /// </summary>
    public class RangeEstimator : IEstimator
    {
        public RangeEstimator(Web3 web3, long lower, long upper)
        {
            Web3 = web3;
            Lower = lower;
            Upper = upper;
        }

        protected Web3 Web3 { get; private set; }

        private long Lower { get; set; }

        private long Upper { get; set; }

        public async Task<BigInteger> EstimateGasPrice()
        {
            HexBigInteger gasPrice = await Web3.Eth.GasPrice.SendRequestAsync();
            return gasPrice.Value > Upper ? Upper :
                (gasPrice.Value < Lower ? Lower : gasPrice.Value);
        }
    }
}
