using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace THNETII.SharePoint.IdentityModel
{
    public class SharePointAuthorizationDiscoveryMetadata
    {
        public const string DomainKey = "domain";
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

            var targetInstance = new SharePointAuthorizationDiscoveryMetadata(
                paramsDict)
            {
                Domain = PopValue(paramsDict, DomainKey),
                Realm = PopValue(paramsDict, RealmKey),
                ResourcePrincipal = PopValue(paramsDict, ResourcePrincipalKey),
                AuthorizationUri = PopValue(paramsDict, AuthorizationUriKey)
            };
            if (PopValue(paramsDict, TrustedIssuersKey) is string trustedIssuers)
            {
                targetInstance.TrustedIssuers.AddRange(trustedIssuers.Split(
                    commaSeparator, StringSplitOptions.RemoveEmptyEntries));
            }
            return targetInstance;

            static string? PopValue(Dictionary<string, string> dict, string key)
            {
                _ = dict.TryGetValue(key, out string? value)
                    && dict.Remove(key);
                return value;
            }
        }

        private SharePointAuthorizationDiscoveryMetadata(
            Dictionary<string, string>? additionalData) : base()
        {
            AdditionalData = additionalData ?? new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        }

        [DebuggerStepThrough]
        public SharePointAuthorizationDiscoveryMetadata() : this(null) { }

        public string? Domain { get; set; }

        public string? Realm { get; set; }

        public string? ResourcePrincipal { get; set; }

        public List<string> TrustedIssuers { get; } = new List<string>(capacity: 4);

        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = "String-based configuration")]
        public string? AuthorizationUri { get; set; }

        public Dictionary<string, string> AdditionalData { get; }

        public string? GetAuthorizationInstance() => AuthorizationUri switch
        {
            string authUri => new Uri(authUri).GetLeftPart(UriPartial.Authority),
            _ => null,
        };

        public string GetQualifiedClientId(string clientId) => clientId + '@' + Realm;

        public string GetResource() => Domain switch
        {
            { Length: int l } when l > 0 => ResourcePrincipal + '/' + Domain + '@' + Realm,
            _ => ResourcePrincipal + '@' + Realm,
        };
    }
}
