using System;
using System.Net.Http;

using Microsoft.IdentityModel.Protocols;

namespace THNETII.SharePoint.AzureAcs.Protocol
{
    public class AcsRealmMetadataManager
        : ConfigurationManager<AcsRealmMetadataDocument>
    {
        public const string DefaultInstance = "https://accounts.accesscontrol.windows.net";
        private static readonly AcsRealmMetadataRetriever defaultConfigRetriever =
            new AcsRealmMetadataRetriever();

        public AcsRealmMetadataManager(
            string realm, HttpClient? httpClient)
            : this(realm, instance: default, httpClient)
        { }

        public AcsRealmMetadataManager(
            string realm, string? instance, HttpClient? httpClient)
            : this(realm, instance ?? DefaultInstance,
                  configRetriever: default,
                  !(httpClient is null) ? new HttpDocumentRetriever(httpClient) : default)
        { }

        public AcsRealmMetadataManager(
            string realm, string? instance = DefaultInstance,
            AcsRealmMetadataRetriever? configRetriever = default,
            HttpDocumentRetriever? documentRetriever = default)
            : base(
                  GetMetadataAddress(realm, instance),
                  configRetriever ?? defaultConfigRetriever,
                  documentRetriever ?? new HttpDocumentRetriever())
        { }

        public static string GetMetadataAddress(string realm, string? instance)
        {
            instance ??= DefaultInstance;
            const string slash = "/";
            const string metadataPath = "metadata/json/1?realm=";
            string address;
            if (instance.AsSpan().EndsWith(slash.AsSpan(), StringComparison.Ordinal))
                address = instance + metadataPath;
            else
                address = instance + slash + metadataPath;
            return address + Uri.EscapeDataString(realm ?? string.Empty);
        }
    }
}
