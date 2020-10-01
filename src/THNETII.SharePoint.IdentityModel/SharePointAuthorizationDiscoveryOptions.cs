using System.Diagnostics.CodeAnalysis;

using Microsoft.IdentityModel.Protocols;

namespace THNETII.SharePoint.IdentityModel
{
    public class SharePointAuthorizationDiscoveryOptions
    {
        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = "String-based configuration")]
        public string SiteUrl { get; set; } = null!;
        public string? MetadataAddress { get; set; }
        public SharePointAuthorizationDiscoveryMetadata? DiscoveryMetadata { get; set; }
        public IConfigurationManager<SharePointAuthorizationDiscoveryMetadata> DiscoveryManager { get; set; } = null!;
        public bool RequireHttpsMetadata { get; set; } = true;
    }
}
