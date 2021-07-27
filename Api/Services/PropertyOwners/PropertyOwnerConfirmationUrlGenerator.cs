﻿using HappyTravel.Edo.Api.Infrastructure.Options;
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
            var encriptedBytes = Convert.FromBase64String(encryptedString);

            return DecryptStringFromBytes_Aes(encriptedBytes, _urlGenerationOptions.AesKey, _urlGenerationOptions.AesIV);
        }


        private string Encrypt(string stringToEncrypt)
        {
            var encriptedBytes = EncryptStringToBytes_Aes(stringToEncrypt, _urlGenerationOptions.AesKey, _urlGenerationOptions.AesIV);

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
        //private static readonly byte[] Key = new byte[32] { 121, 90, 35, 45, 22, 214, 45, 89, 56, 176, 25, 11, 250, 177, 237, 251, 
        //    155, 47, 115, 23, 157, 166, 101, 135, 83, 126, 222, 7, 26, 231, 219, 252 };
        //private static readonly byte[] IV = new byte[16] { 26, 131, 30, 106, 233, 60, 139, 254, 4, 227, 5, 32, 11, 132, 253, 115 };
        //private readonly string constantPartOfUrl = "dev.happytravel.com/confirmation-page";
    }
}
