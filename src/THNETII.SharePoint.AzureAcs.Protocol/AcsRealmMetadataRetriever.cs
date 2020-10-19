using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
#if NETSTANDARD_API_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif
using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;

using THNETII.SharePoint.BearerAuthorization;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsRealmMetadataRetriever
        : IConfigurationRetriever<AcsRealmMetadataDocument>
    {
#if !NETSTANDARD_API_SYSTEM_TEXT_JSON
        private static readonly DataContractJsonSerializer metadataSerializer =
            new DataContractJsonSerializer(typeof(AcsRealmMetadataDocument));
#endif

        public static Task<AcsRealmMetadataDocument>
            GetAsync(string address, CancellationToken cancelToken = default) =>
            GetAsync(address,
                new HttpWwwAuthenticateHeaderParameterRetriever(),
                cancelToken);

        public static Task<AcsRealmMetadataDocument>
            GetAsync(string address, HttpClient httpClient,
                CancellationToken cancelToken = default) =>
            GetAsync(address,
                new HttpWwwAuthenticateHeaderParameterRetriever(httpClient ??
                    throw LogHelper.LogArgumentNullException(nameof(httpClient))),
                cancelToken);

        public static async Task<AcsRealmMetadataDocument>
            GetAsync(string address, IDocumentRetriever documentRetriever,
                CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(address))
                throw LogHelper.LogArgumentNullException(nameof(address));
            if (documentRetriever is null)
                throw LogHelper.LogArgumentNullException(nameof(documentRetriever));

            string metadataJson = await documentRetriever
                .GetDocumentAsync(address, cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);
            LogHelper.LogVerbose(LogMessages.THSP2001, metadataJson);
#if NETSTANDARD_API_SYSTEM_TEXT_JSON
            var metadataDocument = JsonSerializer
                .Deserialize<AcsRealmMetadataDocument>(metadataJson);
#else
            AcsRealmMetadataDocument metadataDocument;
            using (var metadataStream = new MemoryStream(Encoding.UTF8.GetBytes(metadataJson)))
                metadataDocument = (AcsRealmMetadataDocument)metadataSerializer.ReadObject(metadataStream);
#endif
            return metadataDocument;
        }

        [SuppressMessage("Design",
            "CA1033: Interface methods should be callable by child types",
            Justification = nameof(SharePointSiteAuthorizationRetriever))]
        Task<AcsRealmMetadataDocument>
            IConfigurationRetriever<AcsRealmMetadataDocument>
            .GetConfigurationAsync(string address,
                IDocumentRetriever retriever,
                CancellationToken cancelToken) =>
            GetAsync(address, retriever, cancelToken);
    }
}
