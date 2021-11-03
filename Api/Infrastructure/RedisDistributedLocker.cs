using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using StackExchange.Redis;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class RedisDistributedLocker : IDistributedLocker
    {
        public RedisDistributedLocker(IDatabaseAsync redisDb)
        {
            _redisDb = redisDb;
        }


        public async Task<Result> TryAcquireLock(string key, TimeSpan duration)
        {
            var redisKey = $"{nameof(RedisDistributedLocker)}::{key}";
            return await _redisDb.LockTakeAsync(new RedisKey(redisKey), RedisValue.Null, duration)
                ? Result.Success()
                : Result.Failure("Lock was already taken");
        }
        
        
        private readonly IDatabaseAsync _redisDb;
    }
}