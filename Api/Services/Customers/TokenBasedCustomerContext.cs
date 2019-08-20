using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Customers;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class TokenBasedCustomerContext : ICustomerContext
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly ICustomerService _customerService;

        public TokenBasedCustomerContext(IHttpContextAccessor accessor, 
            ICustomerService customerService)
        {
            _accessor = accessor;
            _customerService = customerService;
        }
        
        public async Task<Result<Customer>> GetCurrent()
        {
            var identityClaim = GetCurrentCustomerIdentity();
            if(!(identityClaim is null))
                return await _customerService.GetByIdentityId(identityClaim);

            var clientIdClaim = GetClaimValue("client_id");
            if (!(clientIdClaim is null))
                return await _customerService.GetByClientId(clientIdClaim);

            return Result.Fail<Customer>("Could not find customer");
        }

        public string GetCurrentCustomerIdentity() => GetClaimValue("sub");
        
        private string GetClaimValue(string claimType)
        {
            return _accessor.HttpContext.User.Claims
                .SingleOrDefault(c => c.Type == claimType)
                ?.Value;
        }
    }
}