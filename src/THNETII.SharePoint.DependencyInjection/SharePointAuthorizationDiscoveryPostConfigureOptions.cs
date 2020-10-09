using System;
using System.Net.Http;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;

using THNETII.SharePoint.IdentityModel;

namespace THNETII.SharePoint.DependencyInjection
{
    public class SharePointAuthorizationDiscoveryPostConfigureOptions
        : IPostConfigureOptions<SharePointAuthorizationDiscoveryOptions>
    {
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
                        var httpRetriever = options.DiscoveryHttpClient is HttpClient httpClient
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
