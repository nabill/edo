using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class HashGenerator
    {
        public static string ComputeSha256(string source)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
            var builder = new StringBuilder();
            foreach (var t in bytes)
                builder.Append(t.ToString("x2"));

            return builder.ToString();
        }
        
        
        public static string ComputeHash<T>(T source)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(source);
            return Convert.ToBase64String(bytes);
        }
    }
}