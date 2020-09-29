using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class HttpBasedServiceAccountContext : IServiceAccountContext
    {
        public HttpBasedServiceAccountContext(EdoContext context, ITokenInfoAccessor tokenInfoAccessor)
        {
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
        }


        public async Task<Result<ServiceAccount>> GetCurrent()
        {
            var clientId = _tokenInfoAccessor.GetClientId();
            if (string.IsNullOrWhiteSpace(clientId))
                return Result.Failure<ServiceAccount>("ClientId is empty");

            var account = await _context.ServiceAccounts
                .SingleOrDefaultAsync(c => c.ClientId == clientId);

            return account != default
                ? Result.Success(account)
                : Result.Failure<ServiceAccount>("Could not get service account");
        }


        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}