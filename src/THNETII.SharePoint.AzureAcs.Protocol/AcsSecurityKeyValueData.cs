using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsSecurityKeyValueData
    {
        [JsonProperty("type")]
        public string Type { get; set; } = null!;
        [JsonProperty("value")]
        public string Base64Data { get; set; } = null!;
        [JsonProperty("keyInfo")]
        public IDictionary<string, object> Information { get; } =
            new Dictionary<string, object>(StringComparer.Ordinal);
    }
}
