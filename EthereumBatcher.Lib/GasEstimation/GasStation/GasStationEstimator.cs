using Nethereum.Util;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;

namespace EthereumBatcher.Lib.GasEstimation.GasStation
{
    public enum DesiredSpeed
    {
        SafeLow,
        Average,
        Fast,
        Fastest
    }

    /// <summary>
    /// Ethereum gas station estimator.
    /// </summary>
    /// <remarks>
    ///  Link to site with docs https://ethgasstation.info/
    /// </remarks>
    public class GasStationEstimator
    {
        public GasStationEstimator(string urlPath = "https://ethgasstation.info/json/ethgasAPI.json", DesiredSpeed speed = DesiredSpeed.SafeLow)
        {
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(urlPath)
            };

            Speed = speed;
        }

        protected HttpClient HttpClient { get; set; }

        protected DesiredSpeed Speed { get; set; }

        public async Task<BigInteger> EstimateGasPrice()
        {
            HttpResponseMessage response = await HttpClient.GetAsync(string.Empty);
            EstimationModel estimate = JsonConvert.DeserializeObject<EstimationModel>(await response.Content.ReadAsStringAsync());
            return new UnitConversion().ToWei(estimate.SafeLow / 10, UnitConversion.EthUnit.Gwei);
        }
    }
}
