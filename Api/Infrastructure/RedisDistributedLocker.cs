using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using StackExchange.Redis;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class RedisDistributedLocker : IDistributedLocker
    {
        public RedisDistributedLocker(IConnectionMultiplexer connectionMultiplexer)
        {
            _redisDb = connectionMultiplexer.GetDatabase();
        }


        public async Task<Result> TryAcquireLock(string key, TimeSpan duration)
        {
            var redisKey = $"{nameof(RedisDistributedLocker)}{KeyDelimiter}{key}";
            return await _redisDb.LockTakeAsync(new RedisKey(redisKey), RedisValue.EmptyString, duration)
                ? Result.Success()
                : Result.Failure("Lock was already taken");
        }
        
        
        public Task ReleaseLock(string key)
        {
            var redisKey = $"{nameof(RedisDistributedLocker)}{KeyDelimiter}{key}";
            return _redisDb.LockReleaseAsync(new RedisKey(redisKey), RedisValue.EmptyString);
        }


        private const string KeyDelimiter = "::";
        private readonly IDatabaseAsync _redisDb;
    }
}