using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HappyTravel.Edo.Api.Infrastructure.Environments
{
    public static class EnvironmentVariableHelper
    {
        public static string Get(string key, IConfiguration configuration)
        {
            var environmentVariable = configuration[key];
            if (environmentVariable is null)
                throw new Exception($"Couldn't obtain the value for '{key}' configuration key.");

            return Environment.GetEnvironmentVariable(environmentVariable);
        }


        public static bool IsLocal(this IHostEnvironment hostingEnvironment) 
            => hostingEnvironment.IsEnvironment(LocalEnvironment);  
        
        
        public static int GetPort(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!int.TryParse(value, out var port))
                throw new Exception($"{key} is not set");

            return port;
        }
        

        private const string LocalEnvironment = "Local";
    }
}