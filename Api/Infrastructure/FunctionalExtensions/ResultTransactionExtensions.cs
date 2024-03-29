using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions
{
    public static class ResultTransactionExtensions
    {
        public static Task<Result> BindWithTransaction(
            this Result target,
            DbContext context,
            Func<Task<Result>> f)
        {
            var (_, isFailure, error) = target;
            if (isFailure)
                return Task.FromResult(Result.Failure(error));

            return WithTransactionScope(context, f);
        }
        
        
        public static Task<Result<T>> BindWithTransaction<T>(
            this Result target,
            DbContext context,
            Func<Task<Result<T>>> f)
        {
            var (_, isFailure, error) = target;
            if (isFailure)
                return Task.FromResult(Result.Failure<T>(error));

            return WithTransactionScope(context, f);
        }


        public static async Task<Result> BindWithTransaction(
            this Task<Result> target,
            DbContext context,
            Func<Task<Result>> f)
        {
            var (_, isFailure, error) = await target;
            if (isFailure)
                return Result.Failure(error);

            return await WithTransactionScope(context, f);
        }


        public static async Task<Result<TK>> BindWithTransaction<T, TK>(
            this Task<Result<T>> target,
            DbContext context,
            Func<T, Task<Result<TK>>> f)
        {
            var (_, isFailure, result, error) = await target;
            if (isFailure)
                return Result.Failure<TK>(error);

            return await WithTransactionScope(context, () => f(result));
        }


        public static async Task<Result> BindWithTransaction<T>(
            this Task<Result<T>> target,
            DbContext context,
            Func<T, Task<Result>> f)
        {
            var (_, isFailure, result, error) = await target;
            if (isFailure)
                return Result.Failure(error);

            return await WithTransactionScope(context, () => f(result));
        }


        public static async Task<Result<T>> BindWithTransaction<T>(
            this Task<Result> target,
            DbContext context,
            Func<Task<Result<T>>> f)
        {
            var (_, isFailure, error) = await target;
            if (isFailure)
                return Result.Failure<T>(error);

            return await WithTransactionScope(context, f);
        }


        public static async Task<Result<T>> BindWithTransaction<T, TK>(
            this Result<TK> target,
            DbContext context,
            Func<TK, Task<Result<T>>> f)
        {
            var (_, isFailure, result, error) = target;
            if (isFailure)
                return Result.Failure<T>(error);

            return await WithTransactionScope(context, () => f(result));
        }


        public static async Task<Result<T, TE>> BindWithTransaction<T, TE>(
            this Task<Result<T, TE>> target,
            DbContext context,
            Func<T, Task<Result<T, TE>>> f)
        {
            var (_, isFailure, result, error) = await target;
            if (isFailure)
                return Result.Failure<T, TE>(error);

            return await WithTransactionScope(context, () => f(result));
        }


        public static async Task<Result<TOutput, TE>> BindWithTransaction<TInput, TOutput, TE>(
            this Task<Result<TInput, TE>> target,
            DbContext context,
            Func<TInput, Task<Result<TOutput, TE>>> f)
        {
            var (_, isFailure, result, error) = await target;
            if (isFailure)
                return Result.Failure<TOutput, TE>(error);

            return await WithTransactionScope(context, () => f(result));
        }


        public static async Task<Result> BindWithTransaction<T>(
            this Result<T> target,
            DbContext context,
            Func<T, Task<Result>> f)
        {
            var (_, isFailure, result, error) = target;
            if (isFailure)
                return Result.Failure(error);

            return await WithTransactionScope(context, () => f(result));
        }


        private static Task<TResult> WithTransactionScope<TResult>(DbContext context, Func<Task<TResult>> operation)
            where TResult : IResult
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync((object) null,
                async (dbContext, state, cancellationToken) =>
                {
                    // Nested transaction support. We can commit only on a top-level
                    var transaction = dbContext.Database.CurrentTransaction is null
                        ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
                        : null;
                    try
                    {
                        var result = await operation();
                        if (result.IsSuccess)
                            transaction?.Commit();

                        return result;
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                },
                // This delegate is not used in NpgSql.
                null);
        }
    }
}