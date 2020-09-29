using System.Diagnostics.CodeAnalysis;

namespace THNETII.SharePoint.AuthDiscoveryConsole
{
    public class SharePointAuthorizationDiscoveryOptions
    {
        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = "String-based configuration")]
        public string SiteUrl { get; set; } = null!;
    }
}
