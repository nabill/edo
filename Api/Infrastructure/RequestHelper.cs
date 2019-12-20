using System.IO;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class RequestHelper
    {
        public static async ValueTask<Result<string>> GetAsString(Stream stream)
        {
            try
            {
                using (stream)
                {
                    using (var readStream = new StreamReader(stream, Encoding.UTF8))
                    {
                        return Result.Ok(await readStream.ReadToEndAsync());
                    }
                }
            }
            catch
            {
                return Result.Fail<string>("Cannot read as string.");
            }
        }
    }
}