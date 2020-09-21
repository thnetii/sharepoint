using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;

namespace THNETII.SharePoint.IdentityModel
{
    public class HttpWwwAuthenticateHeaderParameterRetriever : IDocumentRetriever
    {
        private static readonly AuthenticationHeaderValue emptyBearer =
            new AuthenticationHeaderValue("Bearer");
        private static HttpClient? defaultHttpClient;

        private static HttpClient DefaultHttpClient
        {
            get
            {
                if (defaultHttpClient is null)
                    defaultHttpClient = new HttpClient();
                return defaultHttpClient;
            }
        }

        private readonly HttpClient? httpClient;

        public HttpWwwAuthenticateHeaderParameterRetriever() : base() { }

        public HttpWwwAuthenticateHeaderParameterRetriever(
            HttpClient httpClient)
            : this()
        {
            this.httpClient = httpClient
                ?? throw LogHelper.LogArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc cref="HttpDocumentRetriever.RequireHttps"/>
        public bool RequireHttps { get; set; } = true;


        public async Task<string> GetDocumentAsync(string address,
            CancellationToken cancelToken = default)
        {
            Uri uri = ValidateAddress(address);
            var httpClient = this.httpClient ?? DefaultHttpClient;

            LogHelper.LogVerbose(LogMessages.THSP1002, uri);
            using var requ = new HttpRequestMessage(HttpMethod.Head, uri)
            { Headers = { Authorization = emptyBearer } };
            try
            {
                using var resp = await httpClient.SendAsync(requ,
                    HttpCompletionOption.ResponseHeadersRead, cancelToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
                var wwwAuthenticateHeader = resp.Headers.WwwAuthenticate
                .FirstOrDefault(bearerSelectorPredicate);
                return wwwAuthenticateHeader?.Parameter ?? string.Empty;
            }
            catch (Exception httpSendException)
            {
                throw LogHelper.LogExceptionMessage(new IOException(
                    LogHelper.FormatInvariant(LogMessages.THSP1003, address),
                    httpSendException)
                    );
            }
        }

        private Uri ValidateAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw LogHelper.LogArgumentNullException(nameof(address));

            Uri uri;
            try { uri = new Uri(address); }
            catch (UriFormatException uriFormatExcept)
            {
                throw LogHelper.LogException<IOException>(uriFormatExcept,
                    $"");
            }

            if (RequireHttps && !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                throw LogHelper.LogExceptionMessage(new ArgumentException(LogHelper.FormatInvariant(LogMessages.THSP1001, address), nameof(address)));

            return uri;
        }

        private static readonly Func<AuthenticationHeaderValue, bool>
            bearerSelectorPredicate = IsBearerAuthenticationHeader;

        private static bool IsBearerAuthenticationHeader(AuthenticationHeaderValue header) =>
            header.Scheme.Equals(emptyBearer.Scheme, StringComparison.OrdinalIgnoreCase);
    }
}
