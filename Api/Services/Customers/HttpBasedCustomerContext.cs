using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class HttpBasedCustomerContext : ICustomerContext, ICustomerContextInternal
    {
        public HttpBasedCustomerContext(EdoContext context,
            ITokenInfoAccessor tokenInfoAccessor)
        {
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
        }


        public async ValueTask<Result<CustomerInfo>> GetCustomerInfo()
        {
            // TODO: Add caching
            if (!_customerInfo.Equals(default))
                return Result.Ok(_customerInfo);

            _customerInfo = await GetCustomerInfoByIdentityHashOrId();
            
            return _customerInfo.Equals(default) 
                ? Result.Fail<CustomerInfo>("Could not get customer data") 
                : Result.Ok(_customerInfo);
        }


        public async ValueTask<CustomerInfo> GetCustomer()
        {
            var (_, isFailure, customer, error) = await GetCustomerInfo();
            // Normally this should not happen and such error is a signal that something going wrong.
            if(isFailure)
                throw new UnauthorizedAccessException("Customer retrieval failure");

            return customer;
        }


        private async ValueTask<CustomerInfo> GetCustomerInfoByIdentityHashOrId(int customerId = default)
        {
            // TODO: use counterparty information from headers to get counterparty id
            return await (from customer in _context.Customers
                    from customerCounterpartyRelation in _context.CustomerCounterpartyRelations.Where(r => r.CustomerId == customer.Id)
                    from counterparty in _context.Counterparties.Where(c => c.Id == customerCounterpartyRelation.CounterpartyId)
                    from agency in _context.Agencies.Where(b => b.Id == customerCounterpartyRelation.AgencyId)
                    where customerId.Equals(default)
                        ? customer.IdentityHash == GetUserIdentityHash()
                        : customer.Id == customerId
                    select new CustomerInfo(customer.Id,
                        customer.FirstName,
                        customer.LastName,
                        customer.Email,
                        customer.Title,
                        customer.Position,
                        counterparty.Id,
                        counterparty.Name,
                        agency.Id,
                        customerCounterpartyRelation.Type == CustomerCounterpartyRelationTypes.Master,
                        customerCounterpartyRelation.InCounterpartyPermissions))
                .SingleOrDefaultAsync();
        }


        public async Task<Result<UserInfo>> GetUserInfo()
        {
            return (await GetCustomerInfo())
                .OnSuccess(customer => new UserInfo(customer.CustomerId, UserTypes.Customer));
        }


        public async Task<List<CustomerCounterpartyInfo>> GetCustomerCounterparties()
        {
            var (_, isFailure, customerInfo, _) = await GetCustomerInfo();
            if (isFailure)
                return new List<CustomerCounterpartyInfo>(0);

            return await (
                    from cr in _context.CustomerCounterpartyRelations
                    join br in _context.Agencies
                        on cr.AgencyId equals br.Id
                    join co in _context.Counterparties
                        on cr.CounterpartyId equals co.Id
                    where cr.CustomerId == customerInfo.CustomerId
                    select new CustomerCounterpartyInfo(
                        co.Id,
                        co.Name,
                        br.Id,
                        br.Title,
                        cr.Type == CustomerCounterpartyRelationTypes.Master,
                        cr.InCounterpartyPermissions.ToList()))
                .ToListAsync();
        }


        private string GetUserIdentityHash()
        {
            var identityClaim = _tokenInfoAccessor.GetIdentity();
            if (identityClaim != null)
                return HashGenerator.ComputeSha256(identityClaim);

            var clientIdClaim = _tokenInfoAccessor.GetClientId();
            #warning TODO: Remove this after implementing client-customer relation
            if (clientIdClaim != null)
                return clientIdClaim;

            return string.Empty;
        }

        
        //TODO TICKET https://happytravel.atlassian.net/browse/NIJO-314 
        public async ValueTask<Result<CustomerInfo>> SetCustomerInfo(int customerId)
        {
            var customerInfo = await GetCustomerInfoByIdentityHashOrId(customerId);
            if (customerInfo.Equals(default))
                return Result.Fail<CustomerInfo>("Could not set customer data");
            _customerInfo = customerInfo;
            return Result.Ok(_customerInfo);
        }
             
        
        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private CustomerInfo _customerInfo;
    }
}