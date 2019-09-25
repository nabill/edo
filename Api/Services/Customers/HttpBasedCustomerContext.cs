using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class HttpBasedCustomerContext : ICustomerContext
    {
        public HttpBasedCustomerContext(IHttpContextAccessor accessor,
            EdoContext context,
            ITokenInfoAccessor tokenInfoAccessor)
        {
            _accessor = accessor;
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
        }

        public async ValueTask<Result<Customer>> GetCustomer()
        {
            _customer = _customer ?? await GetCustomerFromClaims();
            return _customer is null
                ? Result.Fail<Customer>("Could not find customer")
                : Result.Ok(_customer);
        }

        public async ValueTask<Result<Company>> GetCompany()
        {
            _company = _company ?? await GetCompanyFromHttpContext();
            return _company is null
                ? Result.Fail<Company>("Could not find company")
                : Result.Ok(_company);
        }

        public async ValueTask<bool> IsMasterCustomer()
        {
            var (_, isCustomerFailure, customer, _) = await GetCustomer();
            if(isCustomerFailure)
                return false;
            
            var (_, isCompanyFailure, company, _) = await GetCompany();
            if(isCompanyFailure)
                return false;

            return await _context.CustomerCompanyRelations
                .Where(cr => cr.CustomerId == customer.Id)
                .Where(cr => cr.CompanyId == company.Id)
                .Where(cr => cr.Type == CustomerCompanyRelationTypes.Master)
                .AnyAsync();;
        }

        public async ValueTask<Result<CustomerData>> GetCustomerData()
        {
            var customerResult = await GetCustomer();
            if (customerResult.IsFailure)
                return Result.Fail<CustomerData>(customerResult.Error);
            
            var companyResult = await GetCompany();
            if (companyResult.IsFailure)
                return Result.Fail<CustomerData>(companyResult.Error);
            
            var isMaster = await IsMasterCustomer();
            
            return Result.Ok(new CustomerData(customerResult.Value,
                 companyResult.Value, isMaster));
        }


        private async ValueTask<Company> GetCompanyFromHttpContext()
        {
            // TODO: implement getting company from headers
            var (_, isFailure, customer, _) = await GetCustomer();
            if (isFailure)
                return null;

            return await _context.CustomerCompanyRelations
                .Where(cr => cr.CustomerId == customer.Id)
                .Join(_context.Companies,
                    relation => relation.CompanyId,
                    company => company.Id,
                    (relation, company) => company)
                .SingleOrDefaultAsync();
        }

        private async ValueTask<Customer> GetCustomerFromClaims()
        {
            var identityClaim = _tokenInfoAccessor.GetIdentity();
            if (!(identityClaim is null))
            {
                var identityHash = HashGenerator.ComputeHash(identityClaim);
                return await _context.Customers
                    .SingleOrDefaultAsync(c => c.IdentityHash == identityHash);
            }

            var clientIdClaim = _tokenInfoAccessor.GetClientId();
            if (!(clientIdClaim is null))
            {
#warning TODO: Remove this after implementing client-customer relation
                return await _context.Customers
                    .SingleOrDefaultAsync(c => c.IdentityHash == clientIdClaim);
            }

            return null;
        }
        
        private readonly IHttpContextAccessor _accessor;
        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private Company _company;
        private Customer _customer;
    }
}