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
                // TODO: use company information from headers to get company id
                _customerInfo = await (from customer in _context.Customers
                    from customerCompanyRelation in _context.CustomerCompanyRelations.Where(r => r.CustomerId == customer.Id)
                    from company in _context.Companies.Where(c => c.Id == customerCompanyRelation.CompanyId)
                    from branch in _context.Branches.Where(b => b.Id == customerCompanyRelation.BranchId).DefaultIfEmpty()
                    where customer.IdentityHash == identityHash
                    select new CustomerInfo(customer,
                        company,
                        null, // TODO: change this to branch when EF core issue will be resolved
                        customerCompanyRelation.Type == CustomerCompanyRelationTypes.Master))
                    .SingleOrDefaultAsync();
            }
            
            return _customerInfo.Equals(default)
                ? Result.Fail<CustomerInfo>("Could not get customer data")
                : Result.Ok(_customerInfo);
        }


        public async Task<Result> SetAppSettings(string appSettings)
        {
            var customer = await _context.Customers
                .SingleOrDefaultAsync(c => c.IdentityHash == GetUserIdentityHash());

            if (customer == default)
                return Result.Fail("Could not find customer");

            customer.AppSettings = appSettings;
            _context.Update(customer);
            await _context.SaveChangesAsync();
            
            return Result.Ok();
        }


        public async Task<Result<string>> GetAppSettings()
        {
            var customer = await _context.Customers
                .SingleOrDefaultAsync(c => c.IdentityHash == GetUserIdentityHash());

            return customer == default
                ? Result.Fail<string>("Could not find customer")
                : Result.Ok(customer.AppSettings);
        }


        private string GetUserIdentityHash()
        {
            var identityClaim = _tokenInfoAccessor.GetIdentity();
            string identityHash = null;
            if (!(identityClaim is null))
            {
                return HashGenerator.ComputeHash(identityClaim);
            }

            var clientIdClaim = _tokenInfoAccessor.GetClientId();
            if (!(clientIdClaim is null))
            {
#warning TODO: Remove this after implementing client-customer relation
                return clientIdClaim;
            }

            return string.Empty;
        }

        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private CustomerInfo _customerInfo;
    }
}