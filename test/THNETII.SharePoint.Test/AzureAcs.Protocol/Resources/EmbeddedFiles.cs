using System;
using System.IO;
using System.Text;

#if MICROSOFT_EXTENSIONS_FILEPROVIDERS_EMBEDDED_API
using Microsoft.Extensions.FileProviders;
#else
#endif

namespace THNETII.SharePoint.AzureAcs.Protocol.Resources
{

    public static class EmbeddedFiles
    {
        private static readonly Lazy<string> acsRealmMetadataDocumentLazy =
            new Lazy<string>(ReadAcsRealmMetadataDocument);
        public static string AcsRealmMetadataDocument { get; } =
            acsRealmMetadataDocumentLazy.Value;

#if false && MICROSOFT_EXTENSIONS_FILEPROVIDERS_EMBEDDED_API
        private static readonly EmbeddedFileProvider fileProvider =
            new EmbeddedFileProvider(
                typeof(EmbeddedFiles).Assembly,
                typeof(EmbeddedFiles).Namespace);

        private static string ReadAcsRealmMetadataDocument()
        {
            var fileInfo = fileProvider.GetFileInfo(nameof(AcsRealmMetadataDocument) + ".json");
            using var fileStream = fileInfo.CreateReadStream();
            using var fileReader = new StreamReader(fileStream, Encoding.UTF8);
            return fileReader.ReadToEnd();
        }
#else
        private static string ReadAcsRealmMetadataDocument()
        {
            var thisType = typeof(EmbeddedFiles);

            const string fileName = nameof(AcsRealmMetadataDocument) + ".json";
            using var fileStream = thisType.Assembly.GetManifestResourceStream(thisType, fileName);
            using var fileReader = new StreamReader(fileStream ?? Stream.Null, Encoding.UTF8);
            return fileReader.ReadToEnd();
        }
#endif
    }
}
