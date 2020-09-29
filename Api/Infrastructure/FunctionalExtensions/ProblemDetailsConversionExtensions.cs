using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions
{
    public static class ProblemDetailsConversionExtensions
    {
        public static async Task<Result<T, ProblemDetails>> ToResultWithProblemDetails<T>(this Task<Result<T>> task)
        {
            var (isSuccess, _, result, error) = await task;

            return isSuccess
                ? Result.Success<T, ProblemDetails>(result)
                : ProblemDetailsBuilder.Fail<T>(error);
        }
        
        
        public static async Task<Result<VoidObject, ProblemDetails>> ToResultWithProblemDetails(this Task<Result> task)
        {
            var (isSuccess, _, error) = await task;

            return isSuccess
                ? Result.Success<VoidObject, ProblemDetails>(VoidObject.Instance)
                : ProblemDetailsBuilder.Fail<VoidObject>(error);
        }
    }
}