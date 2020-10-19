using System.Runtime.Serialization;
#if NETSTANDARD_API_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    [DataContract]
    public class AcsSecurityKeyMetadataEntry
    {
        [DataMember(Name = "usage")]
        public string Usage { get; set; } = null!;
        [DataMember(Name = "keyValue")]
        public AcsSecurityKeyValueData Value { get; set; } = null!;
    }
}
