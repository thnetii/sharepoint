using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.IdentityModel.Tokens;

namespace THNETII.SharePoint.BearerAuthorization
{
    public class SharePointSiteAuthorizationMetadata
    {
        public const string DomainKey = "domain";
        public const string RealmKey = "realm";
        public const string ResourcePrincipalKey = "client_id";
        public const string TrustedIssuersKey = "trusted_issuers";
        public const string AuthorizationUriKey = "authorization_uri";
        private static readonly char[] commaSeparator = new[] { ',' };

        public static SharePointSiteAuthorizationMetadata Create(
            string wwwAuthenticateParams)
        {
            var paramsDict = HttpWwwAuthenticateHeaderParameterParser
                .Parse(wwwAuthenticateParams);

            var targetInstance = new SharePointSiteAuthorizationMetadata(
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

        private SharePointSiteAuthorizationMetadata(
            Dictionary<string, string>? additionalData) : base()
        {
            AdditionalData = additionalData ?? new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        }

        [DebuggerStepThrough]
        public SharePointSiteAuthorizationMetadata() : this(null) { }

        public string? Domain { get; set; }

        public string? Realm { get; set; }

        public string? ResourcePrincipal { get; set; }

        public List<string> TrustedIssuers { get; } = new List<string>(capacity: 4);

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

        private static readonly Dictionary<string, Regex> trustedIssuerRegexes =
            new Dictionary<string, Regex>(StringComparer.Ordinal);

        public static IssuerValidator TrustedIssuerValidator { get; } = ValidateIssuer;

        private static string ValidateIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            var trustedIssuers = validationParameters switch
            {
                { ValidIssuer: string singleIssuer }
                when singleIssuer.Length > 0 => new[] { singleIssuer },
                { ValidIssuers: IEnumerable<string> issuers } => issuers,
                _ => Enumerable.Empty<string>(),
            };

            if (trustedIssuers
                .Select(i => GetOrCreateIssuerRegex(i))
                .Any(r => r.IsMatch(issuer)))
                return issuer;

            throw new SecurityTokenInvalidIssuerException
            {
                InvalidIssuer = issuer
            };
        }

        private static Regex GetOrCreateIssuerRegex(string issuer)
        {
            Regex? regex;
            lock (trustedIssuerRegexes)
            {
                if (trustedIssuerRegexes.TryGetValue(issuer, out regex))
                    return regex;
            }
            regex = CreateIssuerRegex(issuer);
            lock (trustedIssuerRegexes)
            {
                trustedIssuerRegexes[issuer] = regex;
            }
            return regex;
        }

        private static Regex CreateIssuerRegex(string issuer)
        {
            var issuerSpan = issuer.AsSpan();
            var regexBuilder = new StringBuilder(issuerSpan.Length + 10);
            for (int asterisk = issuerSpan.IndexOf('*');
                asterisk >= 0;
                issuerSpan =
#if CSHARP_LANG_FEATURE_RANGE_INDEX
                    issuerSpan[(asterisk + 1)..]
#else
                    issuerSpan.Slice(asterisk + 1)
#endif
                    ,
                asterisk = issuerSpan.IndexOf('*'))
            {
                var stringPart =
#if CSHARP_LANG_FEATURE_RANGE_INDEX
                    issuerSpan[..asterisk]
#else
                    issuerSpan.Slice(0, asterisk)
#endif
                    ;
                string regexPart = Regex.Escape(stringPart.ToString());
                regexBuilder.Append(regexPart);
                regexBuilder.Append(".*");
            }
            regexBuilder.Append(issuerSpan
#if !NETSTANDARD_API_STRING_STRINGCOMPARISON
                .ToString()
#endif
                );
            return new Regex(regexBuilder.ToString());
        }

        public void ConfigureAccessTokenValidation(
            TokenValidationParameters parameters)
        {
            if (parameters is null)
                return;

            if (!string.IsNullOrEmpty(Domain))
            {
                parameters.ValidAudience = "https://" + Domain;
                parameters.ValidateAudience = true;
                parameters.IgnoreTrailingSlashWhenValidatingAudience = true;
            }

            parameters.ValidateIssuer = true;
            parameters.ValidIssuer = null;
            parameters.ValidIssuers = TrustedIssuers;
            parameters.IssuerValidator = TrustedIssuerValidator;
        }
    }
}
