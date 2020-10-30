using System;
using System.Globalization;

using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsOAuth2AccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonProperty("expires_in")]
        public string ExpiresInString
        {
            get => ExpiresIn.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            set
            {
                var seconds = double.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                ExpiresIn = TimeSpan.FromSeconds(seconds);
            }
        }

        [JsonIgnore]
        public TimeSpan ExpiresIn { get; set; }

        [JsonProperty("expires_on")]
        public string? ExpiresOnEpochString
        {
            get => GetEpochOffsetStringFromDateTimeOffset(ExpiresOn);
            set => ExpiresOn = GetDateTimeOffsetFromEpochOffsetString(value);
        }

        [JsonIgnore]
        public DateTimeOffset? ExpiresOn { get; set; }

        [JsonProperty("not_before")]
        public string? NotBeforeEpochString
        {
            get => GetEpochOffsetStringFromDateTimeOffset(NotBefore);
            set => NotBefore = GetDateTimeOffsetFromEpochOffsetString(value);
        }

        [JsonIgnore]
        public DateTimeOffset? NotBefore { get; set; }

        [JsonProperty("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string? Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = null!;

        private static string? GetEpochOffsetStringFromDateTimeOffset(DateTimeOffset? value)
        {
            if (!value.HasValue)
                return null;

            long epochOffset = EpochTime.GetIntDate(value.Value.UtcDateTime);
            return epochOffset.ToString(CultureInfo.InvariantCulture);
        }

        private static DateTimeOffset? GetDateTimeOffsetFromEpochOffsetString(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            long epochOffset = long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            return EpochTime.DateTime(epochOffset);
        }
    }
}
