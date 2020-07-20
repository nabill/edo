﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions
{
    public static class ResultLockExtensions
    {
        public static async Task<Result<TOutput>> BindWithLock<TInput, TOutput>(
            this Task<Result<TInput>> target,
            IEntityLocker locker, Func<TInput, Task<Result<TOutput>>> func)
            where TInput : IEntity
        {
            var (_, isFailure, entity, error) = await target.ConfigureAwait(Result.DefaultConfigureAwait);
            return isFailure
                ? Result.Failure<TOutput>(error)
                : await WithLock(locker, GetCaller(), () => func(entity), (typeof(TInput), entity.Id.ToString()));
        }


        public static async Task<Result> BindWithLock<TInput>(
            this Task<Result<TInput>> target,
            IEntityLocker locker, Func<TInput, Task<Result>> func)
            where TInput : IEntity
        {
            var (_, isFailure, entity, error) = await target.ConfigureAwait(Result.DefaultConfigureAwait);
            return isFailure
                ? Result.Failure(error)
                : await WithLock(locker, GetCaller(), () => func(entity).Map(Dummy), (typeof(TInput), entity.Id.ToString()));
        }


        public static async Task<Result<TOutput>> BindWithLock<TInput1, TInput2, TOutput>(
            this Task<Result<(TInput1, TInput2)>> target,
            IEntityLocker locker, Func<(TInput1, TInput2), Task<Result<TOutput>>> func)
            where TInput1 : IEntity 
            where TInput2 : IEntity
        {
            var (_, isFailure, (entity1, entity2), error) = await target.ConfigureAwait(Result.DefaultConfigureAwait);
            return isFailure
                ? Result.Failure<TOutput>(error)
                : await WithLock(locker, GetCaller(), () => func((entity1, entity2)),
                    (typeof(TInput1), entity1.Id.ToString()), (typeof(TInput1), entity2.Id.ToString()));
        }


        public static async Task<Result<TOutput>> BindWithLock<TOutput>(
            this Result target,
            IEntityLocker locker, Type lockType, string lockId, Func<Task<Result<TOutput>>> func)
        {
            return target.IsFailure
                ? Result.Failure<TOutput>(target.Error)
                : await WithLock(locker, GetCaller(), func, (lockType, lockId));
        }


        private static async Task<Result<TResult>> WithLock<TResult>(IEntityLocker locker, string lockerName,
            Func<Task<Result<TResult>>> operation, params (Type, string)[] locks)
        {
            var acquiredLocks = new List<(Type entityType, string entityId)>();

            try
            {
                foreach (var (entityType, entityId) in locks)
                {
                    var (isLockSuccess, _, lockError) = await locker.Acquire(entityType, entityId, lockerName);

                    if (isLockSuccess)
                        acquiredLocks.Add((entityType, entityId));
                    else
                        return Result.Failure<TResult>(lockError);
                }

                return await operation().ConfigureAwait(Result.DefaultConfigureAwait);
            }
            finally
            {
                foreach (var (entityType, entityId) in acquiredLocks)
                    await locker.Release(entityType, entityId);
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetCaller()
        {
            var method = new StackTrace().GetFrame(FrameIndex).GetMethod();
            var methodDeclaringType = method.DeclaringType;

            return $"{methodDeclaringType?.Name}.{method.Name}";
        }


        private static VoidObject Dummy() => VoidObject.Instance;


        private const int FrameIndex = 8;
    }
}
