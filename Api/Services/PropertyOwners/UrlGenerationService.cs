using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HappyTravel.Edo.Api.Services.PropertyOwners
{
    public class UrlGenerationService : IUrlGenerationService
    {
        public string Generate(string referenceCode)
        {
            var variablePartOfUrl = GenerateFromString(referenceCode);

            return $"{constantPartOfUrl}/{variablePartOfUrl}";
        }


        private static string GenerateFromString(string stringToEncrypt)
        //    => Convert.ToBase64String(Encoding.ASCII.GetBytes(referenceCode));
        {
            var encriptedBytes = EncryptStringToBytes_Aes(stringToEncrypt, Key, IV);

            return Convert.ToBase64String(encriptedBytes);
        }


        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.GenerateKey();   // For test only
                aesAlg.GenerateIV();    // For test only

                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }


        private static readonly byte[] Key = new byte[32] { 121, 90, 35, 45, 22, 214, 45, 89, 56, 176, 25, 11, 250, 177, 237, 251, 
            155, 47, 115, 23, 157, 166, 101, 135, 83, 126, 222, 7, 26, 231, 219, 252 };
        private static readonly byte[] IV = new byte[16] { 26, 131, 30, 106, 233, 60, 139, 254, 4, 227, 5, 32, 11, 132, 253, 115 };
        private readonly string constantPartOfUrl = "happytravel.com/confirmation-page";
    }
}
