using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class GenericValidator<T> : AbstractValidator<T>
    {
        public static Result Validate(Action<GenericValidator<T>> configureAction, T entity)
        {
            var validator = new GenericValidator<T>();
            configureAction(validator);
            return validator.GetValidationResult(entity);
        }
        
        
        public static async Task<Result> ValidateAsync(Action<GenericValidator<T>> configureAction, T entity)
        {
            var validator = new GenericValidator<T>();
            configureAction(validator);
            return await validator.GetValidationResultAsync(entity);
        }


        private Result GetValidationResult(T entity)
        {
            var validationResult = base.Validate(entity);
            return validationResult.IsValid
                ? Result.Success()
                : Result.Combine(validationResult
                    .Errors
                    .Select(e => Result.Failure(e.ErrorMessage))
                    .ToArray());
        }
        
        
        private async Task<Result> GetValidationResultAsync(T entity)
        {
            var validationResult = await base.ValidateAsync(entity);
            return validationResult.IsValid
                ? Result.Success()
                : Result.Combine(validationResult
                    .Errors
                    .Select(e => Result.Failure(e.ErrorMessage))
                    .ToArray());
        }
    }
}