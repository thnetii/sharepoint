using System.Threading;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Protocols;

namespace THNETII.SharePoint.IdentityModel.Protocol
{
    internal class InMemoryDocumentRetriever : IDocumentRetriever
    {
        private readonly Task<string> documentTask;

        public InMemoryDocumentRetriever(string document)
        {
            documentTask = Task.FromResult(document);
        }

        public Task<string> GetDocumentAsync(string address, CancellationToken cancel)
        {
            return documentTask;
        }
    }
}
