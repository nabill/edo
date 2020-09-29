using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace HappyTravel.Edo.Api.Infrastructure.Http
{
    public static class RequestHelper
    {
        public static async ValueTask<Result<string>> GetAsString(Stream stream)
        {
            try
            {
                using (var readStream = new StreamReader(stream, Encoding.UTF8))
                {
                    return Result.Success(await readStream.ReadToEndAsync());
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Cannot read http request body.";
                Logger?.LogError(ex, errorMessage);
                return Result.Failure<string>(errorMessage);
            }
        }


        public static async ValueTask<Result<char[]>> GetAsChars(Stream stream)
        {
            var result = await GetAsBytes(stream);
            return result.IsFailure 
                ? Result.Failure<char[]>(result.Error) 
                : Result.Success(Encoding.UTF8.GetChars(result.Value));
        }
        

        public static async ValueTask<Result<byte[]>> GetAsBytes(Stream stream)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    return Result.Success(memoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Cannot read http request body.";
                Logger?.LogError(ex, errorMessage);
                return Result.Failure<byte[]>(errorMessage);
            }
        }


        private static readonly ILogger Logger = Logging.AppLogging
            .CreateLogger("RequestHelper");
    }
}