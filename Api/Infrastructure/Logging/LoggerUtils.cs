using System;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    public static class LoggerUtils
    {
        public static Result WriteLogByResult(Result result, Action logSuccess, Action logFailure)
        {
            if (result.IsSuccess)
                logSuccess();
            else
                logFailure();

            return result;
        }
    }
}