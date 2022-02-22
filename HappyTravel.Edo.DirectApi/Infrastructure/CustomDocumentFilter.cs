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
        var tags = new Dictionary<int, OpenApiTag>();
        foreach(var path in swaggerDoc.Paths)
        {
            var routeAttribute = context.ApiDescriptions
                .FirstOrDefault(x => x.RelativePath
                    .Replace("/", string.Empty)
                    .Equals( path.Key.Replace("/", string.Empty), StringComparison.InvariantCultureIgnoreCase))
                .ActionDescriptor.EndpointMetadata.First(x=>x is RouteAttribute) as RouteAttribute;

            var order = routeAttribute?.Order ?? 0;
            var originalTagName = string.Empty;
            var name = routeAttribute?.Name;

            if (name is not null)
            {
                foreach (var key in path.Value.Operations.Keys)
                {
                    foreach (var tag in path.Value.Operations[key].Tags)
                    {
                        originalTagName = tag.Name ?? string.Empty;
                        tag.Name = name;
                    }
                }
            }

            var rootTag = swaggerDoc.Tags.FirstOrDefault(t => t.Name == originalTagName);
            if (rootTag is not null)
            {
                rootTag.Name = name;
                tags.Add(order, rootTag);
            }

            paths.Add(path, order);
        }

        var orderedPaths = paths.OrderBy(x => x.Value).ToList();
        swaggerDoc.Paths.Clear();
        orderedPaths.ForEach(x => swaggerDoc.Paths.Add(x.Key.Key, x.Key.Value));
        swaggerDoc.Tags = tags.OrderBy(x => x.Key).Select(x => x.Value).ToList();
    }
}