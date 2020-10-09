using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

using Microsoft.IdentityModel.Protocols;

using THNETII.SharePoint.IdentityModel;

namespace THNETII.SharePoint.DependencyInjection
{
    public class SharePointAuthorizationDiscoveryOptions
    {
        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = "String-based configuration")]
        public string SiteUrl { get; set; } = null!;
        public string? MetadataAddress { get; set; }
        public HttpClient? DiscoveryHttpClient { get; set; }
        public SharePointAuthorizationDiscoveryMetadata? DiscoveryMetadata { get; set; }
        public IConfigurationManager<SharePointAuthorizationDiscoveryMetadata> DiscoveryManager { get; set; } = null!;
        public bool RequireHttpsMetadata { get; set; } = true;
    }
}
