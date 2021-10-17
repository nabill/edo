using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    internal static class ConfigureApiVersioningExtension
    {
        public static IServiceCollection ConfigureApiVersioning(this IServiceCollection collection)
        {
            collection.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            
            collection.AddVersionedApiExplorer(options =>
            {
                options.SubstitutionFormat = "V.v";
                options.SubstituteApiVersionInUrl = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });

            return collection;
        }
    }
}