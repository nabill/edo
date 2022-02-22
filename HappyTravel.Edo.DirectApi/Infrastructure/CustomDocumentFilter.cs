using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HappyTravel.Edo.DirectApi.Infrastructure;

public class CustomDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = new Dictionary<KeyValuePair<string, OpenApiPathItem>, int>();
        foreach(var path in swaggerDoc.Paths)
        {
            var routeAttribute = context.ApiDescriptions
                .FirstOrDefault(x => x.RelativePath
                    .Replace("/", string.Empty)
                    .Equals( path.Key.Replace("/", string.Empty), StringComparison.InvariantCultureIgnoreCase))
                .ActionDescriptor.EndpointMetadata.First(x=>x is RouteAttribute) as RouteAttribute;

            var order = routeAttribute?.Order ?? 0;
            paths.Add(path, order);
        }

        var orderedPaths = paths.OrderBy(x => x.Value).ToList();
        swaggerDoc.Paths.Clear();
        orderedPaths.ForEach(x => swaggerDoc.Paths.Add(x.Key.Key, x.Key.Value));
    }
}