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


        private static readonly byte[] Key = new byte[] { 24, 37, 15 };
        private static readonly byte[] IV = new byte[] { 12, 32, 43 };
        private readonly string constantPartOfUrl = "happytravel.com/confirmation-page";
    }
}
