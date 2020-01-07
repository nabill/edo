using System.Linq;

namespace HappyTravel.Edo.Api.Infrastructure.Http.Extensions
{
    public static class HttpRequest
    {
        public static string GetRequestIdHeader(this Microsoft.AspNetCore.Http.HttpRequest httpRequest)
        {
            return httpRequest.Headers.TryGetValue(Constants.Common.RequestIdHeader, out var value) 
                ? value.SingleOrDefault()
                : string.Empty;
        }
    }
}