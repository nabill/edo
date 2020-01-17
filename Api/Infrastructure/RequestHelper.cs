using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure
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


        public static async ValueTask<Result<byte[]>> GetAsBytes(Stream stream)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    return Result.Ok(memoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Cannot read http request body.";
                Logger?.LogError(ex, errorMessage);
                return Result.Fail<byte[]>(errorMessage);
            }
        }


        private static readonly ILogger Logger = Logging.AppLogging
            .CreateLogger("RequestHelper");
    }
}