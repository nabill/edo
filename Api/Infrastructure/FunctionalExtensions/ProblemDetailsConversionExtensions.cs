using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions
{
    public static class ProblemDetailsConversionExtensions
    {
        public static async Task<Result<T, ProblemDetails>> ToResultWithProblemDetails<T>(this Task<Result<T>> task)
        {
            var (isSuccess, _, result, error) = await task;

            return isSuccess
                ? Result.Ok<T, ProblemDetails>(result)
                : ProblemDetailsBuilder.Fail<T>(error);
        }
        
        public static async Task<Result<T>> ToResultWithoutProblemDetails<T>(this Task<Result<T, ProblemDetails>> task)
        {
            var (isSuccess, _, result, error) = await task;

            return isSuccess
                ? Result.Ok<T, ProblemDetails>(result)
                : ProblemDetailsBuilder.Fail<T>(error.Detail);
        }
    }
}