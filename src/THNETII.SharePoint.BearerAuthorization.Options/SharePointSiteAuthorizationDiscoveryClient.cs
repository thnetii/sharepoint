using System.Net.Http;

namespace THNETII.SharePoint.BearerAuthorization
{
    internal class SharePointSiteAuthorizationDiscoveryClient
    {
        internal HttpWwwAuthenticateHeaderParameterRetriever AuthorizationMetadataRetriever { get; }

        internal SharePointSiteAuthorizationDiscoveryClient(HttpClient httpClient)
        {
            AuthorizationMetadataRetriever =
                new HttpWwwAuthenticateHeaderParameterRetriever(httpClient);
        }
    }
}
