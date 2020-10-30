using System;
using System.Collections.Generic;

using Microsoft.IdentityModel.Tokens;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsOAuth2AccessTokenResult
    {
        public AcsOAuth2AccessTokenResult(AcsOAuth2AccessTokenResponse response,
            SecurityToken? validatedToken)
        {
            _ = response ?? throw new ArgumentNullException(nameof(response));

            AccessTokenString = response.AccessToken;
            ExpiresIn = response.ExpiresIn;
            ExpiresOn = response.ExpiresOn;
            NotBefore = response.NotBefore;
            RefreshToken = response.RefreshToken;
            Scopes = response.Scope?.Split(' ');
            TokenType = response.TokenType;
            ValidatedToken = validatedToken;
        }

        public string AccessTokenString { get; }
        public TimeSpan ExpiresIn { get; }
        public DateTimeOffset? ExpiresOn { get; }
        public DateTimeOffset? NotBefore { get; }
        public string? RefreshToken { get; }
        public IEnumerable<string>? Scopes { get; }
        public string TokenType { get; }
        public SecurityToken? ValidatedToken { get; }
    }
}
