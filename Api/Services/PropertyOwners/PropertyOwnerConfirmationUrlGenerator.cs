using HappyTravel.Edo.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography;

namespace HappyTravel.Edo.Api.Services.PropertyOwners
{
    public class PropertyOwnerConfirmationUrlGenerator : IPropertyOwnerConfirmationUrlGenerator
    {
        public PropertyOwnerConfirmationUrlGenerator(IOptions<UrlGenerationOptions> urlGenerationOptions)
        {
            _urlGenerationOptions = urlGenerationOptions.Value;
        }


        public string Generate(string referenceCode)
        {
            var variablePartOfUrl = Encrypt(referenceCode);

            return $"{_urlGenerationOptions.ConfirmationPageUrl}/{variablePartOfUrl}";
        }


        public string ReadReferenceCode(string encryptedReferenceCode)
            => Decrypt(encryptedReferenceCode);


        private string Decrypt(string encryptedString)
        {
            var base64String = Uri.UnescapeDataString(encryptedString);
            Span<byte> bytes = new byte[base64String.Length];
            
            if (Convert.TryFromBase64String(base64String, bytes, out int bytesWritten))
            {
                var encriptedBytes = bytes.Slice(0, bytesWritten).ToArray();

                return DecryptStringFromBytes_Aes(encriptedBytes, _urlGenerationOptions.AesKey, _urlGenerationOptions.AesIV);
            }

            return string.Empty;
        }


        private string Encrypt(string stringToEncrypt)
        {
            var encriptedBytes = EncryptStringToBytes_Aes(stringToEncrypt, _urlGenerationOptions.AesKey, _urlGenerationOptions.AesIV);
            var base64String = Convert.ToBase64String(encriptedBytes);

            return Uri.EscapeDataString(base64String);
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
            string plainText = null;

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
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plainText;
        }


        private readonly UrlGenerationOptions _urlGenerationOptions;
    }
}
