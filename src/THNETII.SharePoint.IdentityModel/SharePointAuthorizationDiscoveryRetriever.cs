using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace THNETII.SharePoint.IdentityModel
{
    public class SharePointAuthorizationDiscoveryRetriever
        : IConfigurationRetriever<SharePointAuthorizationDiscoveryMetadata>
    {
        public static Task<SharePointAuthorizationDiscoveryMetadata>
            GetAsync(string address, CancellationToken cancelToken = default) =>
            GetAsync(address,
                new HttpWwwAuthenticateHeaderParameterRetriever(),
                cancelToken);

        public static Task<SharePointAuthorizationDiscoveryMetadata>
            GetAsync(string address, HttpClient httpClient,
                CancellationToken cancelToken = default) =>
            GetAsync(address,
                new HttpWwwAuthenticateHeaderParameterRetriever(httpClient ??
                    throw LogHelper.LogArgumentNullException(nameof(httpClient))),
                cancelToken);

        public static async Task<SharePointAuthorizationDiscoveryMetadata>
            GetAsync(string address, IDocumentRetriever documentRetriever,
                CancellationToken cancelToken = default)
        {
            if (string.IsNullOrEmpty(address))
                throw LogHelper.LogArgumentNullException(nameof(address));
            if (documentRetriever is null)
                throw LogHelper.LogArgumentNullException(nameof(documentRetriever));

            string wwwAuthenticateParams = await documentRetriever
                .GetDocumentAsync(address, cancelToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            return SharePointAuthorizationDiscoveryMetadata
                .Create(wwwAuthenticateParams);
        }

        [SuppressMessage("Design",
            "CA1033: Interface methods should be callable by child types",
            Justification = nameof(OpenIdConnectConfigurationRetriever))]
        Task<SharePointAuthorizationDiscoveryMetadata>
            IConfigurationRetriever<SharePointAuthorizationDiscoveryMetadata>
            .GetConfigurationAsync(string address,
                IDocumentRetriever retriever,
                CancellationToken cancelToken) =>
            GetAsync(address, retriever, cancelToken);
    }
}
