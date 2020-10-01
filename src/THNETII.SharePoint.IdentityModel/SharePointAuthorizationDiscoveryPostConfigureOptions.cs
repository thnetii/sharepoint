using System;
using System.Net.Http;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;

namespace THNETII.SharePoint.IdentityModel
{
    public class SharePointAuthorizationDiscoveryPostConfigureOptions
        : IPostConfigureOptions<SharePointAuthorizationDiscoveryOptions>
    {
        private readonly IHttpClientFactory? httpClientFactory;

        public SharePointAuthorizationDiscoveryPostConfigureOptions(
            IHttpClientFactory? httpClientFactory = null)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public void PostConfigure(string name, SharePointAuthorizationDiscoveryOptions options)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));

            if (options.DiscoveryManager is null)
            {
                if (options.DiscoveryMetadata is SharePointAuthorizationDiscoveryMetadata metadata)
                {
                    options.DiscoveryManager = new StaticConfigurationManager<SharePointAuthorizationDiscoveryMetadata>(metadata);
                }
                else
                {
                    if (string.IsNullOrEmpty(options.MetadataAddress) &&
                        !string.IsNullOrEmpty(options.SiteUrl))
                    {
                        bool trailingSlash = options.SiteUrl.EndsWith("/", StringComparison.Ordinal);
                        options.MetadataAddress = options.SiteUrl +
                            (trailingSlash ? "" : "/") + "_vti_bin/client.svc";
                    }

                    if (!string.IsNullOrEmpty(options.MetadataAddress))
                    {
                        var httpRetriever = httpClientFactory?.CreateClient(name) is HttpClient httpClient
                            ? new HttpWwwAuthenticateHeaderParameterRetriever(httpClient)
                            : new HttpWwwAuthenticateHeaderParameterRetriever();
                        httpRetriever.RequireHttps = options.RequireHttpsMetadata;
                        options.DiscoveryManager = new ConfigurationManager<SharePointAuthorizationDiscoveryMetadata>(
                            options.MetadataAddress,
                            new SharePointAuthorizationDiscoveryRetriever(),
                            httpRetriever);
                    }
                }
            }
        }
    }
}
