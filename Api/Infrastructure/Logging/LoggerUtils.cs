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
        
        
        public static Result<T> WriteLogByResult<T>(Result<T> result, Action logSuccess, Action logFailure)
        {
            if (result.IsSuccess)
                logSuccess();
            else
                logFailure();
            
            return result;
        }
        
        
        public static Result<T,E> WriteLogByResult<T,E>(Result<T,E> result, Action logSuccess, Action logFailure)
        {
            if (result.IsSuccess)
                logSuccess();
            else
                logFailure();
            
            return result;
        }
    }
}