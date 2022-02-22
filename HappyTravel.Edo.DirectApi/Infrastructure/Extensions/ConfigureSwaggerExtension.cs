using System;
using System.IO;
using System.Reflection;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace HappyTravel.Edo.DirectApi.Infrastructure.Extensions
{
    internal static class ConfigureSwaggerExtension
    {
        public static IServiceCollection ConfigureSwagger(this IServiceCollection collection)
        {
            collection.AddSwaggerGen(c =>
            {
                c.DocInclusionPredicate((_, apiDesc) =>
                {
                    // Hide apis from other assemblies
                    var apiName = apiDesc.ActionDescriptor.DisplayName;
                    var currentName = typeof(Startup).Assembly.GetName().Name;
                    return apiName.StartsWith(currentName);
                });

                var description = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "README.md"));
                c.SwaggerDoc("direct-api", new OpenApiInfo
                {
                    Title = "HappyTravel Api",
                    Description = description
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.CustomSchemaIds(x => x.FullName);
                c.IncludeXmlComments(xmlPath, true);
                c.DocumentFilter<CustomDocumentFilter>();
            });

            collection.AddFluentValidationRulesToSwagger();
            return collection;
        }
    }
}