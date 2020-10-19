using System.Collections.Generic;
using System.Runtime.Serialization;
#if NETSTANDARD_API_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    [DataContract]
    public class AcsRealmMetadataDocument
    {
        [DataMember(Name = "keys")]
        public ICollection<AcsSecurityKeyMetadataEntry> KeyMetadataEntries { get; } =
            new List<AcsSecurityKeyMetadataEntry>();
        [DataMember(Name = "endpoints")]
        public ICollection<AcsRealmEndpointDescriptor> Endpoints { get; } =
            new List<AcsRealmEndpointDescriptor>(capacity: 3);
        [DataMember(Name = "version")]
        public string Version { get; set; } = null!;
        [DataMember(Name = "realm")]
        public string Realm { get; set; } = null!;
        [DataMember(Name = "name")]
        public string Name { get; set; } = null!;
        [DataMember(Name = "allowedAudiences")]
        public ICollection<string> AllowedAudiences { get; } =
            new List<string>();
        [DataMember(Name = "issuer")]
        public string Issuer { get; set; } = null!;
    }
}
