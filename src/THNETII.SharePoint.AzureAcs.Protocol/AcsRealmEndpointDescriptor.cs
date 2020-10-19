using System.Runtime.Serialization;
#if NETSTANDARD_API_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    [DataContract]
    public class AcsRealmEndpointDescriptor
    {
        [DataMember(Name = "location")]
        public string Location { get; set; } = null!;
        [DataMember(Name = "protocol")]
        public string Protocol { get; set; } = null!;
        [DataMember(Name = "usage")]
        public string Usage { get; set; } = null!;
    }
}
