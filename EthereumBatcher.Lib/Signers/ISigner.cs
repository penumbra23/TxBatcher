using System.Threading.Tasks;

namespace EthereumBatcher.Lib.Signers
{
    public interface ISigner<T>
    {
        Task<string> Sign(T transaction);
    }
}
