using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Threading.Tasks;

namespace EthereumBatcher.Lib.Signers
{
    public sealed class EthereumTxSigner : ISigner<TransactionInput>
    {
        public EthereumTxSigner(string privateKey)
        {
            PrivateKey = privateKey;
        }

        private string PrivateKey { get; set; }

        public async Task<string> Sign(TransactionInput transaction)
        {
            return await Task.Run(() =>
                Web3.OfflineTransactionSigner.SignTransaction(
                    PrivateKey,
                    transaction.To,
                    transaction.Value,
                    transaction.Nonce,
                    transaction.GasPrice,
                    transaction.Gas,
                    transaction.Data)
            );
        }
    }
}
