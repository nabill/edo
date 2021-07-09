using HappyTravel.Edo.Api.Services.PropertyOwners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.PropertyOwnerServices
{
    public class UrlGenerationTests
    {
        [Fact]
        public void From_the_resulting_Url_can_extract_reference_code()
        {
            var referenceCode = "DEV-HTL-AE-0001B4-01";
            var url = new UrlGenerationService().Generate(referenceCode);

            var constantPartOfUrl = "happytravel.com/confirmation-page";
            var key = new byte[32] { 121, 90, 35, 45, 22, 214, 45, 89, 56, 176, 25, 11, 250, 177, 237, 251,
                155, 47, 115, 23, 157, 166, 101, 135, 83, 126, 222, 7, 26, 231, 219, 252 };
            var iV = new byte[16] { 26, 131, 30, 106, 233, 60, 139, 254, 4, 227, 5, 32, 11, 132, 253, 115 };
            var stringToDecrypt = url.Substring(constantPartOfUrl.Length + 1);
            var bytesToDecrypt = Convert.FromBase64String(stringToDecrypt);
            var decryptedString = DecryptStringFromBytes_Aes(bytesToDecrypt, key, iV);

            Assert.Equal(referenceCode, decryptedString);
        }


        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
