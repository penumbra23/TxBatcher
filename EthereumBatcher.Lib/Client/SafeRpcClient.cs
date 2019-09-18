using System;
using Common.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;

namespace EthereumBatcher.Lib.Client
{
    /// <summary>
    /// RpcClient offering multiple retries after failed requests.
    /// </summary>
    public class SafeRpcClient : RpcClient
    {
        public SafeRpcClient(
            Uri baseUrl, 
            AuthenticationHeaderValue authHeaderValue = null, 
            JsonSerializerSettings jsonSerializerSettings = null, 
            HttpClientHandler httpClientHandler = null, 
            ILog log = null) 
            : base(baseUrl, authHeaderValue, jsonSerializerSettings, httpClientHandler, log) { }

        /// <summary>
        /// Maximum number of tries before trowing the exception.
        /// </summary>
        public uint CallThreshold { get; set; } = 5;

        public async override Task SendRequestAsync(RpcRequest request, string route = null)
        {
            await SafeCall(async () =>
            {
                await base.SendRequestAsync(request, route).ConfigureAwait(false);
            });
        }

        public async override Task SendRequestAsync(string method, string route = null, params object[] paramList)
        {
            await SafeCall(async () =>
            {
                await base.SendRequestAsync(method, route, paramList).ConfigureAwait(false);
            });
        }

        protected async override Task<RpcResponseMessage> SendAsync(RpcRequestMessage request, string route = null)
        {
            return await SafeCall(async () =>
            {
                return await base.SendAsync(request, route).ConfigureAwait(false);
            });
        }

        protected async override Task<T> SendInnerRequestAsync<T>(RpcRequest request, string route = null)
        {
            return await SafeCall(async () =>
            {
                return await base.SendInnerRequestAsync<T>(request, route).ConfigureAwait(false);
            });
        }

        protected async override Task<T> SendInnerRequestAsync<T>(string method, string route = null, params object[] paramList)
        {
            return await SafeCall(async () =>
            {
                return await base.SendInnerRequestAsync<T>(method, route, paramList).ConfigureAwait(false);
            });
        }

        private async Task<T> SafeCall<T>(Func<Task<T>> delegateCall)
        {
            uint repeats = 0;
            bool expThrown = false;

            Exception lastException;
            do
            {
                try
                {
                    expThrown = false;
                    return await delegateCall();
                }
                catch (Exception ex)
                {
                    if (ex is RpcClientTimeoutException || ex is RpcClientUnknownException)
                    {
                        repeats++;
                        expThrown = true;
                    }
                    lastException = ex;
                }
            } while (repeats < CallThreshold && expThrown);

            throw lastException;
        }

        private async Task SafeCall(Func<Task> delegateCall)
        {
            uint repeats = 0;
            bool expThrown = false;

            Exception lastException = null;

            do
            {
                try
                {
                    expThrown = false;
                    await delegateCall();
                }
                catch (Exception ex)
                {
                    if (ex is RpcClientTimeoutException || ex is RpcClientUnknownException)
                    {
                        lastException = ex;
                        repeats++;
                        expThrown = true;
                    }
                }
            } while (repeats < CallThreshold && expThrown);

            throw lastException;
        }
    }
}
