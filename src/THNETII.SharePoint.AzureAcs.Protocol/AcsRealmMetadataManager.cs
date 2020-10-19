using System;
#if NETSTANDARD_API_SYSTEM_TEXT_JSON
using System.Text.Json;
#endif
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
