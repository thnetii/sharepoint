using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace THNETII.SharePoint.IdentityModel
{
    public class SharePointAuthorizationDiscoveryMetadata
    {
        public const string RealmKey = "realm";
        public const string ResourcePrincipalKey = "client_id";
        public const string TrustedIssuersKey = "trusted_issuers";
        public const string AuthorizationUriKey = "authorization_uri";
        private static readonly char[] commaSeparator = new[] { ',' };

        public static SharePointAuthorizationDiscoveryMetadata Create(
            string wwwAuthenticateParams)
        {
            var paramsDict = HttpWwwAuthenticateHeaderParameterParser
                .Parse(wwwAuthenticateParams);

            _ = paramsDict.TryGetValue(RealmKey, out string? realm)
                && paramsDict.Remove(RealmKey);
            _ = paramsDict.TryGetValue(ResourcePrincipalKey, out string? clientId)
                && paramsDict.Remove(ResourcePrincipalKey);
            _ = paramsDict.TryGetValue(TrustedIssuersKey, out string? trustedIssuers)
                && paramsDict.Remove(TrustedIssuersKey);
            _ = paramsDict.TryGetValue(AuthorizationUriKey, out string? authUri)
                && paramsDict.Remove(AuthorizationUriKey);

            var targetInstance = new SharePointAuthorizationDiscoveryMetadata(
                paramsDict)
            {
                Realm = realm,
                ResourcePrincipal = clientId,
                AuthorizationUri = authUri
            };
            if (trustedIssuers?.Split(
                    commaSeparator, StringSplitOptions.RemoveEmptyEntries
                ) is string[] trustedIssuersList)
            {
                targetInstance.TrustedIssuers.AddRange(trustedIssuersList);
            }
            return targetInstance;
        }

        private SharePointAuthorizationDiscoveryMetadata(
            Dictionary<string, string>? additionalData) : base()
        {
            AdditionalData = additionalData ?? new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        }

        [DebuggerStepThrough]
        public SharePointAuthorizationDiscoveryMetadata() : this(null) { }

        public string? Realm { get; set; }

        public string? ResourcePrincipal { get; set; }

        public List<string> TrustedIssuers { get; } = new List<string>(capacity: 4);

        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = "String-based configuration")]
        public string? AuthorizationUri { get; set; }

        public Dictionary<string, string> AdditionalData { get; }
    }
}
