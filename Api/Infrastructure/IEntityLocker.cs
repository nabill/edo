using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IEntityLocker
    {
        Task<Result> Acquire<TEntity>(string entityId, string lockerName);

        Task<Result> Acquire(Type entityType, string entityId, string lockerName);

        Task Release<TEntity>(string entityId);

        Task Release(Type entityType, string entityId);
    }
}