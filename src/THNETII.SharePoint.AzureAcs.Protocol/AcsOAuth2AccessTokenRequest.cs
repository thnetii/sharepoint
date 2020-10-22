using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsOAuth2AccessTokenRequest
    {
        private readonly Dictionary<string, string> message =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public string Password
        {
            get => message["password"];
            set => message["password"] = value;
        }

        public string RefreshToken
        {
            get => message["refresh_token"];
            set => message["refresh_token"] = value;
        }

        public string Resource
        {
            get => message["resource"];
            set => message["resource"] = value;
        }

        public string Scope
        {
            get => message["scope"];
            set => message["scope"] = value;
        }

        public string AppContext
        {
            get => message["AppContext"];
            set => message["AppContext"] = value;
        }

        public string Assertion
        {
            get => message["assertion"];
            set => message["assertion"] = value;
        }

        public string GrantType
        {
            get => message["grant_type"];
            set => message["grant_type"] = value;
        }

        public string ClientId
        {
            get => message["client_id"];
            set => message["client_id"] = value;
        }

        public string ClientSecret
        {
            get => message["client_secret"];
            set => message["client_secret"] = value;
        }

        public string AuthorizationCode
        {
            get => message["code"];
            set => message["code"] = value;
        }


        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = nameof(FormUrlEncodedContent))]
        public string RedirectUri
        {
            get => message["redirect_uri"];
            set => message["redirect_uri"] = value;
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

        public FormUrlEncodedContent GetFormContent() =>
            new FormUrlEncodedContent(message);
    }
}
