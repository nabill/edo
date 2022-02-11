using System.Collections.Generic;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    internal static class ConfigureAuthenticationExtension
    {
        public static IServiceCollection ConfigureAuthentication(this IServiceCollection collection, Dictionary<string, string> authorityOptions)
        { 
            collection.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authorityOptions["authorityUrl"];
                    options.Audience = "direct_api";
                    options.RequireHttpsMetadata = true;
                });

            return collection;
        }
    }
}