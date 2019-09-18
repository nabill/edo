using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IEntityLocker
    {
        Task<Result> Acquire<TEntity>(int id, string locker);
        Task Release<TEntity>(int entityId);
    }
}