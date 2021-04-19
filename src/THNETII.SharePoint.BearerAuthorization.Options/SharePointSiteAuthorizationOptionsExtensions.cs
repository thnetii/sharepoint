using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace THNETII.SharePoint.BearerAuthorization
{
    public static class SharePointSiteAuthorizationOptionsExtensions
    {
        public static OptionsBuilder<TOptions> PostConfigureSharePointAuthorizationDiscovery
            <TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
            where TOptions : SharePointSiteAuthorizationOptions
        {
            _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));

            optionsBuilder.Services.AddHttpClient<SharePointSiteAuthorizationDiscoveryClient>();
            optionsBuilder.Services.AddTransient<IPostConfigureOptions<TOptions>, SharePointSiteAuthorizationPostConfigureOptions<TOptions>>();

            return optionsBuilder;
        }

        public static OptionsBuilder<TOptions> AddAdminSiteOptions
            <TOptions>(this OptionsBuilder<TOptions> optionsBuilder,
                Action<TOptions>? configureAdminOptions = null)
            where TOptions : SharePointSiteAuthorizationOptions =>
            optionsBuilder.AddNestedNamedOptions(
                SharePointSiteAuthorizationOptions.AdminSiteOptionsName,
                configureAdminOptions,
                GetAdminUrlFromSiteUrl
                );

        //public static OptionsBuilder<TOptions> AddMySiteOptions
        //    <TOptions>(this OptionsBuilder<TOptions> optionsBuilder,
        //        Action<TOptions> configureAdminOptions)
        //    where TOptions : SharePointSiteAuthorizationOptions =>
        //    optionsBuilder.AddNestedNamedOptions(
        //        SharePointSiteAuthorizationOptions.MySiteOptionsName,
        //        configureAdminOptions,
        //        GetMySiteUrlFromSiteUrl
        //        );

        private static OptionsBuilder<TOptions> AddNestedNamedOptions
            <TOptions>(this OptionsBuilder<TOptions> optionsBuilder,
                string nestedOptionsName, Action<TOptions>? configureNestedOptions,
                Func<string, string> nestedSiteUrlTransform)
            where TOptions : SharePointSiteAuthorizationOptions
        {
            _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));
            if (optionsBuilder.Name.Equals(nestedOptionsName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    message: "The nested options name must be different from the root options.",
                    paramName: nameof(nestedOptionsName)
                    );

            var nestedOptionsBuilder = optionsBuilder.Services
                .AddOptions<TOptions>(nestedOptionsName);
            nestedOptionsBuilder.Configure<IOptionsSnapshot<TOptions>>((nestedOptions, snapshot) =>
            {
                var rootOptions = snapshot.Get(optionsBuilder.Name);
                nestedOptions.SiteUrl = nestedSiteUrlTransform(rootOptions.SiteUrl);
            });
            if (configureNestedOptions is not null)
                nestedOptionsBuilder.Configure(configureNestedOptions);

            return optionsBuilder;
        }

        private const string SharePointOnlineAdminSitePrefixSuffix = "-admin";
        private const string SharePointOnlineMySitePrefixSuffix = "-my";
        private const string SharePointOnlineHostSuffix = ".sharepoint.com";

        private static string GetSharepointOnlinePrefix(string siteUrl)
        {
            var uriBuilder = new UriBuilder(siteUrl);
            var siteHost = uriBuilder.Host.AsSpan();
            StripSuffix(ref siteHost, SharePointOnlineHostSuffix, requireSuffix: true);
            StripSuffix(ref siteHost, SharePointOnlineAdminSitePrefixSuffix);
            StripSuffix(ref siteHost, SharePointOnlineMySitePrefixSuffix);

            return siteHost.ToString();

            static void StripSuffix(ref ReadOnlySpan<char> value, string suffix, bool requireSuffix = false)
            {
                if (value.EndsWith(suffix.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    value =
#if CSHARP_LANG_FEATURE_RANGE_INDEX
                    value[..^(SharePointOnlineMySitePrefixSuffix.Length)]
#else
                    value.Slice(0, value.Length - suffix.Length)
#endif
                    ;
                }
                else if (requireSuffix)
                    value = ReadOnlySpan<char>.Empty;
            }
        }

        private static string GetAdminUrlFromSiteUrl(string siteUrl)
        {
            return GetSharepointOnlinePrefix(siteUrl) switch
            {
                { Length: 0 } => siteUrl,
                string prefix => $"https://{prefix}{SharePointOnlineAdminSitePrefixSuffix}{SharePointOnlineHostSuffix}",
            };
        }

        private static string GetMySiteUrlFromSiteUrl(string siteUrl)
        {
            return GetSharepointOnlinePrefix(siteUrl) switch
            {
                { Length: 0 } => siteUrl,
                string prefix => $"https://{prefix}{SharePointOnlineMySitePrefixSuffix}{SharePointOnlineHostSuffix}",
            };
        }
    }
}
