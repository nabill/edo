using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class ClientReferenceCodeValidationService
    {
        public ClientReferenceCodeValidationService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result> Validate(string clientReferenceCode, AgentContext agent)
        {
            if (string.IsNullOrWhiteSpace(clientReferenceCode))
                return Result.Failure("ClientReferenceCode is required");

            var isExists = await _context.Bookings
                .AnyAsync(b => b.ClientReferenceCode == clientReferenceCode && b.AgentId == agent.AgentId);

            return isExists
                ? Result.Failure($"`{clientReferenceCode}` already used")
                : Result.Success();
        }


        private readonly EdoContext _context;
    }
}