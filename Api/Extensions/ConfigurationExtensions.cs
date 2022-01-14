using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class ConfigurationExtensions
    {
        public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
        {
            var value = configuration.GetValue<T>(key);
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new Exception($"Value for `{key}` is not set");

            return value;
        }
    }
}