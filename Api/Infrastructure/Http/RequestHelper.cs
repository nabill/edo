namespace HappyTravel.Edo.Api.Infrastructure.Http
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Microsoft.Extensions.Logging;
     using ILogger = Microsoft.Extensions.Logging.ILogger;

    namespace HappyTravel.NetstormingConnector.Api.Infrastructure
    {
        public static class RequestHelper
        {
            public static async ValueTask<Result<string>> GetAsString(Stream stream)
            {
                try
                {
                    using (var readStream = new StreamReader(stream, Encoding.UTF8))
                    {
                        return Result.Ok(await readStream.ReadToEndAsync());
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = "Cannot read http request body.";
                    Logger?.LogError(ex, errorMessage);
                    return Result.Fail<string>(errorMessage);
                }
            }


            public static async ValueTask<Result<char[]>> GetAsChars(Stream stream)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        var bytes = memoryStream.ToArray();
                        var chars = Encoding.UTF8.GetChars(bytes);
                        return Result.Ok(chars);
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = "Cannot read http request body.";
                    Logger?.LogError(ex, errorMessage);
                    return Result.Fail<char[]>(errorMessage);
                }
            }


            private static readonly ILogger Logger = Logging.AppLogging
                .CreateLogger("RequestHelper");
        }
    }
}