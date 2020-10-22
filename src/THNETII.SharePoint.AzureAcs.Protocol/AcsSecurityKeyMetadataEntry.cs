using Newtonsoft.Json;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsSecurityKeyMetadataEntry
    {
        [JsonProperty("usage")]
        public string Usage { get; set; } = null!;
        [JsonProperty("keyValue")]
        public AcsSecurityKeyValueData Value { get; set; } = null!;
    }
}
