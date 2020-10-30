using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

using Microsoft.IdentityModel.Tokens;

using THNETII.SharePoint.BearerAuthorization;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsOAuth2AccessTokenRequest
    {
        private readonly Dictionary<string, string> message =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public string? Password
        {
            get => message.TryGetValue("password", out string? value) ? value : default;
            set => SetOrRemoveProperty("password", value);
        }

        public string? RefreshToken
        {
            get => message.TryGetValue("refresh_token", out string? value) ? value : default;
            set => SetOrRemoveProperty("refresh_token", value);
        }

        public string? Resource
        {
            get => message.TryGetValue("resource", out string? value) ? value : default;
            set => SetOrRemoveProperty("resource", value);
        }

        public string? Scope
        {
            get => message.TryGetValue("scope", out string? value) ? value : default;
            set => SetOrRemoveProperty("scope", value);
        }


        [SuppressMessage("Maintainability",
            "CA1507: Use nameof to express symbol names",
            Justification = "Special name")]
        public string? AppContext
        {
            get => message.TryGetValue("AppContext", out string? value) ? value : default;
            set => SetOrRemoveProperty("AppContext", value);
        }

        public string? Assertion
        {
            get => message.TryGetValue("assertion", out string? value) ? value : default;
            set => SetOrRemoveProperty("assertion", value);
        }

        public string? GrantType
        {
            get => message.TryGetValue("grant_type", out string? value) ? value : default;
            set => SetOrRemoveProperty("grant_type", value);
        }

        public string? ClientId
        {
            get => message.TryGetValue("client_id", out string? value) ? value : default;
            set => SetOrRemoveProperty("client_id", value);
        }

        public string? ClientSecret
        {
            get => message.TryGetValue("client_secret", out string? value) ? value : default;
            set => SetOrRemoveProperty("client_secret", value);
        }

        public string? AuthorizationCode
        {
            get => message.TryGetValue("code", out string? value) ? value : default;
            set => SetOrRemoveProperty("code", value);
        }


        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = nameof(FormUrlEncodedContent))]
        public string? RedirectUri
        {
            get => message.TryGetValue("redirect_uri", out string? value) ? value : default;
            set => SetOrRemoveProperty("redirect_uri", value);
        }

        public void SetCustomProperty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty", nameof(propertyName));
            }

            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ArgumentException($"'{nameof(propertyValue)}' cannot be null or empty", nameof(propertyValue));
            }

            message[propertyName] = propertyValue;
        }

        private void SetOrRemoveProperty(string propertyName, string? propertyValue)
        {
            if (propertyValue is null)
                message.Remove(propertyName);
            else
                message[propertyName] = propertyValue;
        }

        public FormUrlEncodedContent GetFormContent() =>
            new FormUrlEncodedContent(message);

        public static AcsOAuth2AccessTokenRequest CreateWithAuthorizationCode(
            SharePointSiteAuthorizationMetadata authorizationMetadata,
            string clientId, string clientSecret,
            string authorizationCode,
            Uri? redirectUri)
        {
            _ = authorizationMetadata ?? throw new ArgumentNullException(nameof(authorizationMetadata));

            var request = new AcsOAuth2AccessTokenRequest
            {
                GrantType = AcsOAuth2AccessTokenGrantType.AuthorizationCode,
                ClientId = authorizationMetadata.GetQualifiedClientId(clientId),
                ClientSecret = clientSecret,
                AuthorizationCode = authorizationCode,
                Resource = authorizationMetadata.GetResource(),
            };
            if (!(redirectUri is null))
                request.RedirectUri = redirectUri.AbsoluteUri;
            return request;
        }

        public static AcsOAuth2AccessTokenRequest CreateWithRefreshToken(
            SharePointSiteAuthorizationMetadata authorizationMetadata,
            string clientId, string clientSecret,
            string refreshToken)
        {
            _ = authorizationMetadata ?? throw new ArgumentNullException(nameof(authorizationMetadata));

            var request = new AcsOAuth2AccessTokenRequest
            {
                GrantType = AcsOAuth2AccessTokenGrantType.RefreshToken,
                ClientId = authorizationMetadata.GetQualifiedClientId(clientId),
                ClientSecret = clientSecret,
                RefreshToken = refreshToken,
                Resource = authorizationMetadata.GetResource(),
            };
            return request;
        }

        public static AcsOAuth2AccessTokenRequest CreateWithAppOnlyCredentials(
            SharePointSiteAuthorizationMetadata authorizationMetadata,
            string clientId, string clientSecret,
            IEnumerable<string>? scopes = default)
        {
            _ = authorizationMetadata ?? throw new ArgumentNullException(nameof(authorizationMetadata));

            var request = new AcsOAuth2AccessTokenRequest
            {
                GrantType = AcsOAuth2AccessTokenGrantType.AppOnlyCredentials,
                ClientId = authorizationMetadata.GetQualifiedClientId(clientId),
                ClientSecret = clientSecret,
                Resource = authorizationMetadata.GetResource(),
                Scope = scopes is null ? null! : string.Join(" ", scopes),
            };
            return request;
        }

        public static AcsOAuth2AccessTokenRequest CreateWithClientAssertion(
            SharePointSiteAuthorizationMetadata authorizationMetadata,
            SecurityToken clientAssertion,
            AcsRealmMetadataDocument? acsRealmMetadata = null)
        {
            _ = authorizationMetadata ?? throw new ArgumentNullException(nameof(authorizationMetadata));
            _ = clientAssertion ?? throw new ArgumentNullException(nameof(clientAssertion));

            var request = clientAssertion switch
            {
                JwtSecurityToken jwtAssertions => CreateWithClientAssertion(
                    jwtAssertions, acsRealmMetadata),
                _ => throw new ArgumentException(
                    "Unsupported security token type: " + clientAssertion.GetType().FullName,
                    nameof(clientAssertion)),
            };
            request.Resource = authorizationMetadata.GetResource();
            return request;
        }

        private static AcsOAuth2AccessTokenRequest CreateWithClientAssertion(
            JwtSecurityToken jwtAssertion,
            AcsRealmMetadataDocument? acsRealmMetadata)
        {
            if (!(acsRealmMetadata is null))
            {
                //var validationParameters = new TokenValidationParameters();
                //acsRealmMetadata.ConfigureClientAssertionValidation(validationParameters);
            }

            return new AcsOAuth2AccessTokenRequest
            {
                GrantType = AcsOAuth2AccessTokenGrantType.JwtBearerClientAssertion,
                Assertion = jwtAssertion.ToString(),
            };
        }
    }
}
