using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.PropertyOwners;
using Microsoft.Extensions.Options;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.PropertyOwnerServices
{
    public class UrlGenerationTests
    {
        [Fact]
        public void From_the_resulting_Url_can_extract_reference_code()
        {
            var referenceCode = "DEV-HTL-AE-0001B4-01";
            var urlGenerationOptions = GetValidOptions();
            var urlGenerationService = new PropertyOwnerConfirmationUrlGenerator(urlGenerationOptions);
            var url = urlGenerationService.Generate(referenceCode);

            var stringToDecrypt = url.Substring(urlGenerationOptions.Value.ConfirmationPageUrl.Length + 1);
            var decryptedString = urlGenerationService.ReadReferenceCode(stringToDecrypt);

            Assert.Equal(referenceCode, decryptedString);


            IOptions<UrlGenerationOptions> GetValidOptions()
                => Options.Create(new UrlGenerationOptions
                {
                    ConfirmationPageUrl = "dev.happytravel.com/confirmation-page",
                    AesKey = new byte[32] { 121, 90, 35, 45, 22, 214, 45, 89, 56, 176, 25, 11, 250, 177, 237, 251,
                        155, 47, 115, 23, 157, 166, 101, 135, 83, 126, 222, 7, 26, 231, 219, 252 },
                    AesIV = new byte[16] { 26, 131, 30, 106, 233, 60, 139, 254, 4, 227, 5, 32, 11, 132, 253, 115 }
                });
        }
    }
}
