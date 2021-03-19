using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;

namespace THNETII.SharePoint.BearerAuthorization
{
    public class SharePointSiteAuthorizationPostConfigureOptions<TOptions>
        : IPostConfigureOptions<TOptions>
        where TOptions : SharePointSiteAuthorizationOptions
    {
        private readonly IServiceProvider serviceProvider;

        public SharePointSiteAuthorizationPostConfigureOptions(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void PostConfigure(string name, TOptions options)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));

            if (options.AuthorizationDiscoveryManager is null)
            {
                if (options.AuthorizationDiscovery is not null)
                {
                    options.AuthorizationDiscoveryManager = new StaticConfigurationManager<SharePointSiteAuthorizationMetadata>(options.AuthorizationDiscovery);
                }
                else
                {
                    var discoveryClient = serviceProvider.GetService<SharePointSiteAuthorizationDiscoveryClient>();
                    var discoveryRetriever = discoveryClient?.AuthorizationMetadataRetriever;
                    if (discoveryRetriever is not null)
                    {
                        discoveryRetriever.RequireHttps = options.RequireHttpsDiscovery;
                        options.AuthorizationDiscoveryManager =
                            new SharePointSiteAuthorizationManager(options.SiteUrl, discoveryRetriever);
                    }
                    else
                    {
                        options.AuthorizationDiscoveryManager =
                            new SharePointSiteAuthorizationManager(options.SiteUrl);
                    }
                }
            }
        }
    }
}
