using System.Diagnostics.CodeAnalysis;

using Microsoft.IdentityModel.Protocols;

namespace THNETII.SharePoint.BearerAuthorization
{
    public abstract class SharePointSiteAuthorizationOptions
    {
        public const string AdminSiteOptionsName = "Admin";
        public const string MySiteOptionsName = "MySite";

        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = nameof(Microsoft.Extensions.Options))]
        public virtual string SiteUrl { get; set; } = null!;

        public bool RequireHttpsDiscovery { get; set; } = true;
        public SharePointSiteAuthorizationMetadata? AuthorizationDiscovery { get; set; }
        public IConfigurationManager<SharePointSiteAuthorizationMetadata> AuthorizationDiscoveryManager { get; set; } = null!;
    }
}
