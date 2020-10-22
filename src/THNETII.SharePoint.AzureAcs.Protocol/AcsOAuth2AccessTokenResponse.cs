using System;
using System.Globalization;

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

#if !SYSTEM_DATETIMEOFFSET_UNIXTIME_API
        private static readonly DateTimeOffset epoch = new DateTimeOffset(
            1970, 01, 01, 00, 00, 00, TimeSpan.Zero
            );
#endif

        private static string? GetEpochOffsetStringFromDateTimeOffset(DateTimeOffset? value)
        {
            if (!value.HasValue)
                return null;

            long epochOffset;
#if SYSTEM_DATETIMEOFFSET_UNIXTIME_API
            epochOffset = value.Value.ToUnixTimeSeconds();
#else // !SYSTEM_DATETIMEOFFSET_UNIXTIME_API
            epochOffset = (long)(value.Value - epoch).TotalSeconds;
#endif

            return epochOffset.ToString(CultureInfo.InvariantCulture);
        }

        private static DateTimeOffset? GetDateTimeOffsetFromEpochOffsetString(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

#if SYSTEM_DATETIMEOFFSET_UNIXTIME_API
            long epochOffset = long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            return DateTimeOffset.FromUnixTimeSeconds(epochOffset);
#else // !SYSTEM_DATETIMEOFFSET_UNIXTIME_API
            double epochOffset = double.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            return epoch + TimeSpan.FromSeconds(epochOffset);
#endif
        }
    }
}
