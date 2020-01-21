using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
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

        
        private async ValueTask<Result<CustomerInfo>> GetCustomerInfo(Expression<Func<CustomerAndCompanyRelation, bool>> predicate)
        {
            // TODO: use company information from headers to get company id
            var customerInfo = await (_context.Customers
                    .Join(_context.CustomerCompanyRelations, customer => customer.Id, relation => relation.CustomerId, (customer, relation) => new CustomerAndCompanyRelation()
                    {
                        Customer = customer,
                        Relation = relation
                    })
                    .Where(predicate)
                    .Join(_context.Companies, customerAndRelation => customerAndRelation.Relation.CompanyId, company => company.Id,
                        (customerAndRelation, company) => new
                        {
                            customerAndRelation,
                            company
                        })
                    .GroupJoin(_context.Branches, customerAndRelationAndCompany => customerAndRelationAndCompany.company.Id, branch => branch.Id,
                        (customerAndRelationAndCompany, branches) => new
                        {
                            customerAndRelationAndCompany,
                            branches
                        })
                    .SelectMany(customerAndRelationAndCompanyAndBranches => customerAndRelationAndCompanyAndBranches.branches.DefaultIfEmpty(),
                        (customerAndRelationAndCompanyAndBranches, branchItem)
                            => new CustomerInfo(customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Customer.Id,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Customer.FirstName,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Customer.LastName,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Customer.Email,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Customer.Title,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Customer.Position,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.company.Id,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.company.Name,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Relation.BranchId,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Relation.Type ==
                                CustomerCompanyRelationTypes.Master,
                                customerAndRelationAndCompanyAndBranches.customerAndRelationAndCompany.customerAndRelation.Relation.InCompanyPermissions)))
                .SingleOrDefaultAsync();
            
            return customerInfo.Equals(default)
                ? Result.Fail<CustomerInfo>("Could not get customer data")
                : Result.Ok(customerInfo);
        }
        

        public async ValueTask<Result<CustomerInfo>> GetCustomerInfo()
        {
            // TODO: Add caching
            if (!_customerInfo.Equals(default))
                return Result.Ok(_customerInfo);

            var identityHash = GetUserIdentityHash();
            
            Expression<Func<CustomerAndCompanyRelation, bool>> predicate = customerAndCompanyRelation
                => customerAndCompanyRelation.Customer.IdentityHash == identityHash;
            
            var (_, isFailure, customerInfo, error ) = await GetCustomerInfo(predicate);
                
            if (isFailure)
                return Result.Fail<CustomerInfo>(error);

            _customerInfo = customerInfo;

            return Result.Ok(_customerInfo);
        }


        private ValueTask<Result<CustomerInfo>> GetCustomerInfo(int customerId)
        {
            Expression<Func<CustomerAndCompanyRelation, bool>> predicate = customerAndCompanyRelation
                => customerAndCompanyRelation.Customer.Id == customerId;
            
            return GetCustomerInfo(predicate);
        }
       

        public async ValueTask<Result<CustomerInfo>> SetCustomerInfo(int customerId)
        {
            var (_, isFailure, customerData, error) = await GetCustomerInfo(customerId);
            if (isFailure)
                return Result.Fail<CustomerInfo>(error);
            
            if (customerData.Equals(default))
                return Result.Fail<CustomerInfo>(error);

            _customerInfo = customerData;

            return Result.Ok(_customerInfo);
        }

        
        public async Task<Result<UserInfo>> GetUserInfo()
        {
            return (await GetCustomerInfo())
                .OnSuccess(customer => new UserInfo(customer.CustomerId, UserTypes.Customer));
        }


        public async Task<List<CustomerCompanyInfo>> GetCustomerCompanies()
        {
            var (_, isFailure, customerInfo, _) = await GetCustomerInfo();
            if (isFailure)
                return new List<CustomerCompanyInfo>(0);

            return await _context.CustomerCompanyRelations
                .Where(cr => cr.CustomerId == customerInfo.CustomerId)
                .Join(_context.Companies, cr => cr.CompanyId, company => company.Id, (cr, company) => new CustomerCompanyInfo(
                    company.Id,
                    company.Name,
                    cr.Type == CustomerCompanyRelationTypes.Master,
                    cr.InCompanyPermissions.ToList()))
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


        private class CustomerAndCompanyRelation
        {
            public Customer Customer { get; set; } 
            public CustomerCompanyRelation Relation { get; set; }
        }
        
        
        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private CustomerInfo _customerInfo;
    }
}