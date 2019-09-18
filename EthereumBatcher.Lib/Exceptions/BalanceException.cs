using System;

namespace EthereumBatcher.Lib.Exceptions
{
    public class BalanceException : Exception
    {
        public BalanceException() : base() { }

        public BalanceException(string message) : base(message) { }

        public BalanceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
