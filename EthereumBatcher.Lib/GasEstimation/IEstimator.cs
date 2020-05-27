using System.Numerics;
using System.Threading.Tasks;

namespace EthereumBatcher.Lib.GasEstimation
{
    public interface IEstimator
    {
        Task<BigInteger> EstimateGasPrice();
    }
}
