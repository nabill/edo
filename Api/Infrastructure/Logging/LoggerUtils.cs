using System;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    public static class LoggerUtils
    {
        public static Result<T, ProblemDetails> WriteLogByResult<T>(Result<T, ProblemDetails> result, Action logSuccess, Action logFailure)
        {
            if (result.IsSuccess)
                logSuccess();
            else
                logFailure();

            return result;
        }
    }
}