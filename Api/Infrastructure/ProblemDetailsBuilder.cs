using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public static class ProblemDetailsBuilder
    {
        public static ProblemDetails Build(string details, HttpStatusCode statusCode = HttpStatusCode.BadRequest) 
            => new ProblemDetails
            {
                Detail = details,
                Status = (int) statusCode
            };
    }
}
