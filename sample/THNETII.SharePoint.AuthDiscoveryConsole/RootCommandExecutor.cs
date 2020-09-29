using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using THNETII.CommandLine.Hosting;
using THNETII.SharePoint.IdentityModel;

using static THNETII.SharePoint.IdentityModel.SharePointAuthorizationDiscoveryMetadata;

namespace THNETII.SharePoint.AuthDiscoveryConsole
{
    public sealed class RootCommandExecutor : ICommandLineExecutor, IDisposable
    {
        private readonly ILogger logger;
        private readonly HttpClient httpClient;
        private readonly SharePointAuthorizationDiscoveryOptions options;

        public RootCommandExecutor(
            IOptions<SharePointAuthorizationDiscoveryOptions> options,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory? loggerFactory = null)
        {
            _ = httpClientFactory
                ?? throw new ArgumentNullException(nameof(httpClientFactory));
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;

            this.options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            httpClient = httpClientFactory.CreateClient();
            logger = loggerFactory.CreateLogger(typeof(Program).Namespace);
        }

        public async Task<int> RunAsync(CancellationToken cancelToken = default)
        {
            var siteUrl = options.SiteUrl;
            var metadataAddress = siteUrl
                + (siteUrl.EndsWith("/", StringComparison.Ordinal) ? "" : "/")
                + "_vti_bin/client.svc";

            var metadata = await SharePointAuthorizationDiscoveryRetriever
                .GetAsync(metadataAddress, httpClient, cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            var messageText = new StringBuilder();
            var messageArgs = new List<object?>();

            using (logger.BeginScope(metadata))
            {
                MessageAdd(nameof(metadata.Realm), RealmKey, metadata.Realm, messageText, messageArgs);
                MessageAdd(nameof(metadata.ResourcePrincipal), ResourcePrincipalKey, metadata.ResourcePrincipal, messageText, messageArgs);
                messageText.Append(MessageLine(nameof(metadata.TrustedIssuers), TrustedIssuersKey));
                messageArgs.Add(metadata.TrustedIssuers.ToString());
                messageText.Append($" ({nameof(metadata.TrustedIssuers.Count)}: {{{nameof(metadata.TrustedIssuers)}_{nameof(metadata.TrustedIssuers.Count)}}})");
                messageArgs.Add(metadata.TrustedIssuers.Count);
                int idxIss = 0;
                foreach (string iss in metadata.TrustedIssuers)
                {
                    messageText.AppendLine();
                    messageText.Append($"- {{{TrustedIssuersKey}_{idxIss}}}");
                    messageArgs.Add(iss);
                    idxIss++;
                }
                messageText.AppendLine();
                MessageAdd(nameof(metadata.AuthorizationUri), AuthorizationUriKey, metadata.AuthorizationUri, messageText, messageArgs);
                messageText.Append(MessageLine(nameof(metadata.AdditionalData), nameof(metadata.AdditionalData)));
                messageArgs.Add(metadata.AdditionalData.ToString());
                messageText.Append($" ({nameof(metadata.AdditionalData.Count)}: {{{nameof(metadata.AdditionalData)}_{nameof(metadata.AdditionalData.Count)}}})");
                messageArgs.Add(metadata.AdditionalData.Count);
                foreach (var addKvp in metadata.AdditionalData)
                {
                    messageText.AppendLine();
                    messageText.Append($"- {addKvp.Key}: {{{addKvp.Key}}}");
                    messageArgs.Add(addKvp.Value);
                }
                messageText.AppendLine();

                string message = messageText.ToString().Trim();
                logger.LogInformation(1, message, messageArgs.ToArray());
                messageText.Clear();
                messageArgs.Clear();

                MessageAdd("Instance", "Instance", metadata.GetAuthorizationInstance(), messageText, messageArgs);
                MessageAdd("Resource", "Resource", metadata.GetResource(), messageText, messageArgs);

                message = messageText.ToString().Trim();
                logger.LogInformation(2, message, messageArgs.ToArray());
            }

            return 0;

            static string MessageLine(string name, string key) => $"{name}: {{{key}}}";
            static void MessageAdd(string name, string key, object? value, StringBuilder text, List<object?> args)
            {
                text.AppendLine(MessageLine(name, key));
                args.Add(value);
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
