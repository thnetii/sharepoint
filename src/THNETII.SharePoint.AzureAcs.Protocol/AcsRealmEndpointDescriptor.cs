using Newtonsoft.Json;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsRealmEndpointDescriptor
    {
        [JsonProperty("location")]
        public string Location { get; set; } = null!;
        [JsonProperty("protocol")]
        public string Protocol { get; set; } = null!;
        [JsonProperty("usage")]
        public string Usage { get; set; } = null!;
    }
}
