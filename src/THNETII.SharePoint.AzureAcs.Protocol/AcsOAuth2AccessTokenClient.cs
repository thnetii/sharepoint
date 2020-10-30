using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using THNETII.SharePoint.BearerAuthorization;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsOAuth2AccessTokenClient
    {
        private readonly HttpClient httpClient;

        public AcsOAuth2AccessTokenClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public bool ValidateAccessToken { get; set; } = true;

        public async Task<AcsOAuth2AccessTokenResult> IssueAccessToken(
            SharePointSiteAuthorizationMetadata authorizationMetadata,
            AcsRealmMetadataDocument acsRealmMetadata,
            AcsOAuth2AccessTokenRequest accessTokenRequest,
            CancellationToken cancelToken = default)
        {
            _ = acsRealmMetadata ?? throw new ArgumentNullException(nameof(acsRealmMetadata));
            _ = accessTokenRequest ?? throw new ArgumentNullException(nameof(accessTokenRequest));

            var tokenUri = acsRealmMetadata.GetOAuth2TokenEndpoint();
            using var requestContent = accessTokenRequest.GetFormContent();
            using var responseMessage = await httpClient
                .PostAsync(tokenUri, requestContent, cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
            var responseString = await responseMessage.Content.ReadAsStringAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            var responseJsonObject = JsonConvert.DeserializeObject<JObject>(responseString);
            if (responseJsonObject.ContainsKey("error"))
            {
                var oauth2Error = responseJsonObject
                    .ToObject<AcsOAuth2ErrorResponse>()!;
                throw new SecurityTokenException(oauth2Error.Description)
                {
                    Data = {
                        ["ResponseContent"] = responseString
                    },
                    HelpLink = oauth2Error.ErrorUri,
                };
            }

            var accessTokenResponse = responseJsonObject
                .ToObject<AcsOAuth2AccessTokenResponse>()!;

            var validationParams = new TokenValidationParameters();
            var jwtHandler = new JwtSecurityTokenHandler();
            SecurityToken accessToken;
            if (ValidateAccessToken)
            {
                if (!(authorizationMetadata is null))
                    authorizationMetadata.ConfigureAccessTokenValidation(validationParams);
                acsRealmMetadata.ConfigureAccessTokenValidation(validationParams);

                var claims = jwtHandler.ValidateToken(
                    accessTokenResponse.AccessToken,
                    validationParams, out accessToken);
            }
            else
            {
                accessToken = jwtHandler.ReadJwtToken(accessTokenResponse.AccessToken);
            }

            return new AcsOAuth2AccessTokenResult(accessTokenResponse, accessToken);
        }
    }
}
