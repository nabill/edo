using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Stubble.Core.Builders;

namespace HappyTravel.LogDelegatesGenerator
{
    public static class Generator
    {
        public static string Generate(IEnumerable<LogEventDescriptor> events, string @namespace)
        {
            var template = LoadTemplate();

            return new StubbleBuilder()
                .Build()
                .Render(template, new LogEventsData
                {
                    Descriptions = events.ToArray(),
                    Namespace = @namespace
                });


            static string LoadTemplate()
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.LoggerExtensions.Mustache");
                if(stream is null)
                    throw new FileNotFoundException("Template not found");
                
                using var reader = new StreamReader(stream);

                return reader.ReadToEnd();
            }
        }
    }
}