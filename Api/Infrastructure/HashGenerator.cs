using System.Security.Cryptography;
using System.Text;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class HashGenerator
    {
        public static string ComputeHash(string source)
        {
            using (SHA256 sha256Hash = SHA256.Create())  
            {  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(source));  
                var builder = new StringBuilder();  
                for (int i = 0; i < bytes.Length; i++)  
                {  
                    builder.Append(bytes[i].ToString("x2"));  
                }  
                return builder.ToString();  
            }  
        }
    }
}