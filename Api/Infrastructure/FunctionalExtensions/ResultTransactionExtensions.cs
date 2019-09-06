using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions
{
    public static class ResultTransactionExtensions
    {
        public static Task<Result<T>> OnSuccessWithTransaction<T>(
            this Result self,
            EdoContext context,
            Func<Task<Result<T>>> f)
        {
            return WithTransactionScope(context, () => self.OnSuccess(f));
        }
        
        public static Task<Result<K>> OnSuccessWithTransaction<T, K>(
            this Task<Result<T>> self,
            EdoContext context,
            Func<T, Task<Result<K>>> f)
        {
            return WithTransactionScope(context, (() => self.OnSuccess(f)));
        }
        
        private static Task<TResult> WithTransactionScope<TResult>(EdoContext context, Func<Task<TResult>> operation) 
            where TResult : IResult
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync((object)null,
                operation: async (dbContext, state, cancellationToken) =>
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
                    {
                        var result = await operation().ConfigureAwait(Result.DefaultConfigureAwait);
                        if (result.IsSuccess)
                            transaction.Commit();

                        return result;
                    }
                },
                // This delegate is not used in NpgSql.
                verifySucceeded: null);
        }
    }
}