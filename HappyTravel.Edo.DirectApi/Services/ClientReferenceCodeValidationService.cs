using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class ClientReferenceCodeValidationService
    {
        public ClientReferenceCodeValidationService(EdoContext context, IDistributedLocker locker)
        {
            _context = context;
            _locker = locker;
        }


        public async Task<Result> Validate(string clientReferenceCode, AgentContext agent)
        {
            if (string.IsNullOrWhiteSpace(clientReferenceCode))
                return Result.Failure("ClientReferenceCode is required");
            
            var lockResult = await _locker.TryAcquireLock($"{nameof(ClientReferenceCodeValidationService)}::{clientReferenceCode}", TimeSpan.FromSeconds(LockDurationSeconds));
            if(lockResult.IsFailure)
                return lockResult;

            var isExists = await _context.Bookings
                .AnyAsync(b => b.ClientReferenceCode == clientReferenceCode && b.AgentId == agent.AgentId);

            return isExists
                ? Result.Failure($"`{clientReferenceCode}` already used")
                : Result.Success();
        }


        private const int LockDurationSeconds = 90;


        private readonly EdoContext _context;
        private readonly IDistributedLocker _locker;
    }
}