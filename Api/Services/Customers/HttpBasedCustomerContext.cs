using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class HttpBasedCustomerContext : ICustomerContext
    {
        public HttpBasedCustomerContext(EdoContext context,
            ITokenInfoAccessor tokenInfoAccessor)
        {
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
        }

        public async ValueTask<Result<CustomerInfo>> GetCustomerInfo()
        {
            if (_customerInfo.Equals(default))
            {
                var identityHash = GetUserIdentityHash();
                _customerInfo = await (from customer in _context.Customers
                    from customerCompanyRelation in _context.CustomerCompanyRelations.Where(r => r.CustomerId == customer.Id)
                    from company in _context.Companies.Where(c => c.Id == customerCompanyRelation.CompanyId)
                    from branch in _context.Branches.Where(b => b.Id == customerCompanyRelation.BranchId)
                    where customer.IdentityHash == identityHash
                    select new CustomerInfo(customer,
                        company,
                        branch,
                        customerCompanyRelation.Type == CustomerCompanyRelationTypes.Master))
                    .SingleOrDefaultAsync();
            }
            
            return _customerInfo.Equals(default)
                ? Result.Fail<CustomerInfo>("Could not get customer data")
                : Result.Ok(_customerInfo);
        }

        private string GetUserIdentityHash()
        {
            var identityClaim = _tokenInfoAccessor.GetIdentity();
            string identityHash = null;
            if (!(identityClaim is null))
            {
                identityHash = HashGenerator.ComputeHash(identityClaim);
            }

            var clientIdClaim = _tokenInfoAccessor.GetClientId();
            if (!(clientIdClaim is null))
            {
#warning TODO: Remove this after implementing client-customer relation
                identityHash = clientIdClaim;
            }

            return identityHash;
        }

        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private CustomerInfo _customerInfo;
    }
}