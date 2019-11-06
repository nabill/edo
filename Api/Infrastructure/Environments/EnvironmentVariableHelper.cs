using System;
using Microsoft.Extensions.Configuration;

namespace HappyTravel.Edo.Api.Infrastructure.Environments
{
    public static class EnvironmentVariableHelper
    {
        public static string GetFromEnvironment(string key, IConfiguration configuration)
        {
            var environmentVariable = configuration[key];
            if (environmentVariable is null)
                throw new Exception($"Couldn't obtain the value for '{key}' configuration key.");

            return Environment.GetEnvironmentVariable(environmentVariable);
        }
    }
}
