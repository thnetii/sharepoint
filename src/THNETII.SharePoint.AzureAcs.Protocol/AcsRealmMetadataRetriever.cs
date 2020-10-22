using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;

using Newtonsoft.Json;

using THNETII.SharePoint.BearerAuthorization;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsRealmMetadataRetriever
        : IConfigurationRetriever<AcsRealmMetadataDocument>
    {
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
            return JsonConvert.DeserializeObject<AcsRealmMetadataDocument>(metadataJson);
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
