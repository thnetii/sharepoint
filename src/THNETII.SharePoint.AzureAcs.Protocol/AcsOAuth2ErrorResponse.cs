using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Newtonsoft.Json;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsOAuth2ErrorResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; } = null!;

        [JsonProperty("error_description")]
        public string? Description { get; set; }

        [JsonProperty("error_codes")]
        public IList<int> ErrorCodes { get; } = new List<int>(1);

        [JsonProperty("timestamp")]
        public string? TimestampString
        {
            get => Timestamp.HasValue ? Timestamp.Value.ToString("u", CultureInfo.InvariantCulture) : null;
            set => Timestamp = value is null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        [JsonIgnore]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("trace_id")]
        public string TraceId { get; set; } = null!;

        [JsonProperty("correlation_id")]
        public string CorrelationId { get; set; } = null!;

        [JsonProperty("error_uri")]
        [SuppressMessage("Design",
            "CA1056: URI-like properties should not be strings",
            Justification = nameof(JsonConvert))]
        public string? ErrorUri { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; } =
            new Dictionary<string, object>(StringComparer.Ordinal);
    }
}
