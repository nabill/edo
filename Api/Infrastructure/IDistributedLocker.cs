using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IDistributedLocker
    {
        Task<Result> TryAcquireLock(string key, TimeSpan duration);

        Task ReleaseLock(string key);
    }
}