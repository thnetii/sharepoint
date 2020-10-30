namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public static class AcsOAuth2AccessTokenGrantType
    {
        public const string AuthorizationCode = "authorization_code";
        public const string RefreshToken = "refresh_token";
        public const string AppOnlyCredentials = "client_credentials";
        public const string Saml1ClientAssertion = "urn:oasis:names:tc:SAML:1.0:assertion";
        public const string Saml2ClientAssertion = "urn:oasis:names:tc:SAML:2.0:assertion";
        public const string JwtBearerClientAssertion = "http://oauth.net/grant_type/jwt/1.0/bearer";
    }
}
