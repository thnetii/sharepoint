using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if NETSTANDARD_API_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    [DataContract]
    public class AcsSecurityKeyValueData
    {
        [DataMember(Name = "type")]
        public string Type { get; set; } = null!;
        [DataMember(Name = "value")]
        public string Base64Data { get; set; } = null!;
        [DataMember(Name = "keyInfo")]
        public IDictionary<string, object> Information { get; } =
            new Dictionary<string, object>(StringComparer.Ordinal);
    }
}
