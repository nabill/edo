using System.IO;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Locations;
using Microsoft.Extensions.Logging;

namespace HappyTravel.LogDelegatesGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var descriptions = new[]
            {
                new LogEventDescriptor(LoggerEvents.GeocoderException, LogLevel.Critical, nameof(GoogleGeoCoder), true),
                new LogEventDescriptor(LoggerEvents.DataProviderRequestError, LogLevel.Error, nameof(DataProvider), false)
            };

            var result = Generator.Generate(descriptions, "HappyTravel.Edo.Api.Infrastructure.Logging");
            File.WriteAllText(@"D:\repos\happytravel\edo-api\HappyTravel.LogDelegatesGenerator\LoggerExtensions.cs", result);
        }
    }
}