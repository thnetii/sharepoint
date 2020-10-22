using System;
using System.Linq;

using THNETII.SharePoint.AzureAcs.Protocol.Resources;
using THNETII.SharePoint.IdentityModel.Protocol;

using Xunit;

namespace THNETII.SharePoint.AzureAcs.Protocol.Test
{
    public static class AcsMetadataRetrieverTest
    {
        [Fact]
        public static void GetsSerializedMetadataDocumentFromInMemoryRetriever()
        {
            string document = EmbeddedFiles.AcsRealmMetadataDocument;
            var documentRetriever = new InMemoryDocumentRetriever(document);

            var metadata = AcsRealmMetadataRetriever.GetAsync(
                "https://example.org/" + nameof(EmbeddedFiles.AcsRealmMetadataDocument),
                documentRetriever)
                .GetAwaiter().GetResult();

            Assert.NotNull(metadata);
            Assert.Equal("1.0", metadata.Version);
            Assert.Equal("72416f3c-199e-43f6-9bb5-29f4a10665d5", metadata.Realm);
            Assert.Equal("sts", metadata.Name);
            Assert.Equal("00000001-0000-0000-c000-000000000000@72416f3c-199e-43f6-9bb5-29f4a10665d5", metadata.Issuer);
            Assert.Equal(new[]
            {
                "00000001-0000-0000-c000-000000000000/accounts.accesscontrol.windows.net@fredrikhoeisaetherhotmail.onmicrosoft.com",
                "00000001-0000-0000-c000-000000000000/accounts.accesscontrol.windows.net@72416f3c-199e-43f6-9bb5-29f4a10665d5"
            }, metadata.AllowedAudiences, StringComparer.Ordinal);
            var oauth2EndpointDescr = metadata.Endpoints.FirstOrDefault(e =>
                e.Protocol.Equals("OAuth2", StringComparison.OrdinalIgnoreCase) &&
                e.Usage.Equals("issuance", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(oauth2EndpointDescr);
            Assert.NotEmpty(metadata.KeyMetadataEntries);
            Assert.All(metadata.KeyMetadataEntries, i =>
            {
                Assert.NotEmpty(i.Usage);

                var value = i.Value;
                Assert.NotNull(value);
                Assert.Contains(value.Type, new[] { "x509Certificate" },
                    StringComparer.OrdinalIgnoreCase);
                var data = Convert.FromBase64String(value.Base64Data);
                Assert.NotNull(data);

                Assert.NotEmpty(value.Information);
            });
        }
    }
}
