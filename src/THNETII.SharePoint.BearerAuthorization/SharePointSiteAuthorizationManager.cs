using System;
using System.Net.Http;

using Microsoft.IdentityModel.Protocols;

namespace THNETII.SharePoint.BearerAuthorization
{
    public class SharePointSiteAuthorizationManager
        : ConfigurationManager<SharePointSiteAuthorizationMetadata>
    {
        public SharePointSiteAuthorizationManager(string siteUrl)
            : this(siteUrl, new SharePointSiteAuthorizationRetriever(),
                  new HttpWwwAuthenticateHeaderParameterRetriever())
        { }

        public SharePointSiteAuthorizationManager(
            string siteUrl,
            HttpClient httpClient)
            : this(siteUrl, new SharePointSiteAuthorizationRetriever(),
                  new HttpWwwAuthenticateHeaderParameterRetriever(httpClient))
        { }

        public SharePointSiteAuthorizationManager(
            string siteUrl,
            HttpWwwAuthenticateHeaderParameterRetriever httpRetriever)
            : this(siteUrl, new SharePointSiteAuthorizationRetriever(), httpRetriever) { }

        public SharePointSiteAuthorizationManager(
            string siteUrl,
            SharePointSiteAuthorizationRetriever metaRetriever,
            HttpWwwAuthenticateHeaderParameterRetriever httpRetriever)
            : base(GetMetadataAddress(siteUrl), metaRetriever, httpRetriever) { }

        public static string GetMetadataAddress(string siteUrl)
        {
            const string slash = "/";
            const string vtiBinClientSvc = "_vti_bin/client.svc";
            if (siteUrl.AsSpan().EndsWith(slash.AsSpan(), StringComparison.Ordinal))
                return siteUrl + vtiBinClientSvc;
            return siteUrl + slash + vtiBinClientSvc;
        }
    }
}
