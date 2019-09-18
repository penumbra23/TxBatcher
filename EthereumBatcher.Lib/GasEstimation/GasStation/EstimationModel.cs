using Newtonsoft.Json;

namespace EthereumBatcher.Lib.GasEstimation.GasStation
{
    /// <summary>
    /// API model for gas station estimate endpoint.
    /// </summary>
    public class EstimationModel
    {
        [JsonProperty("fast")]
        public double Fast { get; set; }

        [JsonProperty("fastest")]
        public double Fastest { get; set; }

        [JsonProperty("safeLow")]
        public double SafeLow { get; set; }

        [JsonProperty("average")]
        public double Average { get; set; }

        [JsonProperty("block_time")]
        public double BlockTime { get; set; }

        [JsonProperty("blockNum")]
        public long BlockNum { get; set; }

        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("safeLowWait")]
        public double SafeLowWait { get; set; }

        [JsonProperty("avgWait")]
        public double AvgWait { get; set; }

        [JsonProperty("fastWait")]
        public double FastWait { get; set; }

        [JsonProperty("fastestWait")]
        public double FastestWait { get; set; }
    }
}
