using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        public CustomerService(EdoContext context, IDateTimeProvider dateTimeProvider, ICustomerContext customerContext)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _customerContext = customerContext;
        }


        public async Task<Result<Customer>> Add(CustomerRegistrationInfo customerRegistration,
            string externalIdentity,
            string email)
        {
            var (_, isFailure, error) = await Validate(customerRegistration, externalIdentity);
            if (isFailure)
                return Result.Fail<Customer>(error);

            var createdCustomer = new Customer
            {
                Title = customerRegistration.Title,
                FirstName = customerRegistration.FirstName,
                LastName = customerRegistration.LastName,
                Position = customerRegistration.Position,
                Email = email,
                IdentityHash = HashGenerator.ComputeSha256(externalIdentity),
                Created = _dateTimeProvider.UtcNow()
            };

            _context.Customers.Add(createdCustomer);
            await _context.SaveChangesAsync();

            return Result.Ok(createdCustomer);
        }


        public async Task<Result<Customer>> GetMasterCustomer(int companyId)
        {
            var master = await (from c in _context.Customers
                join rel in _context.CustomerCompanyRelations on c.Id equals rel.CustomerId
                where rel.CompanyId == companyId && rel.Type == CustomerCompanyRelationTypes.Master
                select c).FirstOrDefaultAsync();

            if (master is null)
                return Result.Fail<Customer>("Master customer does not exist");

            return Result.Ok(master);
        }

        public async Task<Result<List<CustomerInfoSlim>>> GetCustomers(int companyId, int branchId = default)
        {
            var currentCustomer = await _customerContext.GetCustomer();
            var (_, isFailure, error) = CheckCompanyAndBranch(currentCustomer, companyId, branchId);
            if (isFailure)
                return Result.Fail<List<CustomerInfoSlim>>(error);

            var query = from relation in _context.CustomerCompanyRelations
                join customer in _context.Customers
                    on relation.CustomerId equals customer.Id
                join company in _context.Companies
                    on relation.CompanyId equals company.Id
                join branch in _context.Branches
                    on relation.BranchId equals branch.Id
                join mp in _context.MarkupPolicies
                    on relation.CustomerId equals mp.CustomerId into mpTemporary 
                from policy in mpTemporary.DefaultIfEmpty()
                where branchId == default ? relation.CompanyId == companyId : relation.BranchId == branchId
                select new {relation, customer, company, branch, policy};

            var results = (await query.ToListAsync()).Select(o => 
                new CustomerInfoSlim(o.customer.Id, o.customer.FirstName, o.customer.LastName,
                    o.company.Id, o.company.Name, o.branch.Id, o.branch.Title,
                    GetMarkup(o.policy, o.relation)))
                .ToList();

            return Result.Ok(results);

            MarkupPolicySettings? GetMarkup(MarkupPolicy policy, CustomerCompanyRelation relation)
            {
                if (policy == null)
                    return null;

                // TODO this needs to be reworked once branches become ierarchic
                if (currentCustomer.InCompanyPermissions.HasFlag(InCompanyPermissions.ObserveMarkupInCompany)
                    || currentCustomer.InCompanyPermissions.HasFlag(InCompanyPermissions.ObserveMarkupInBranch) && relation.BranchId == branchId)
                    return new MarkupPolicySettings(policy.Description, policy.TemplateId,
                        policy.TemplateSettings, policy.Order, policy.Currency);

                return null;
            }
        }


        public async Task<Result<CustomerInfo>> GetCustomer(int companyId, int branchId, int customerId)
        {
            var customer = await _customerContext.GetCustomer();
            var (_, isFailure, error) = CheckCompanyAndBranch(customer, companyId, branchId);
            if (isFailure)
                return Result.Fail<CustomerInfo>(error);

            // TODO this needs to be reworked when customers will be able to belong to more than one branch within a company
            var foundCustomer = await (
                    from cr in _context.CustomerCompanyRelations
                    join c in _context.Customers
                        on cr.CustomerId equals c.Id
                    join co in _context.Companies
                        on cr.CompanyId equals co.Id
                    where (branchId == default ? cr.CompanyId == companyId : cr.BranchId == branchId)
                        && cr.CustomerId == customerId
                    select (CustomerInfo?) new CustomerInfo(c.Id, c.FirstName, c.LastName, c.Email, c.Title, c.Position, co.Id, co.Name, cr.BranchId,
                        cr.Type == CustomerCompanyRelationTypes.Master, cr.InCompanyPermissions))
                .SingleOrDefaultAsync();

            if (foundCustomer == null)
                return Result.Fail<CustomerInfo>("Customer not found in specified company or branch");

            return Result.Ok(foundCustomer.Value);
        }


        public async Task<Result<List<InCompanyPermissions>>> UpdateCustomerPermissions(int companyId, int branchId, int customerId, 
            List<InCompanyPermissions> permissions)
        {
            var customer = await _customerContext.GetCustomer();

            return await CheckPermission()
                .OnSuccess(() => CheckCompanyAndBranch(customer, companyId,
                    customer.InCompanyPermissions.HasFlag(InCompanyPermissions.PermissionManagementInCompany) ? default : branchId))
                .OnSuccess(GetRelation)
                .OnSuccess(UpdatePermissions);

            Result CheckPermission()
            {
                if (!customer.InCompanyPermissions.HasFlag(InCompanyPermissions.PermissionManagementInBranch)
                    && !customer.InCompanyPermissions.HasFlag(InCompanyPermissions.PermissionManagementInCompany))
                    return Result.Fail("Permission to update customers permissions denied");

                return Result.Ok();
            }

            async Task<Result<CustomerCompanyRelation>> GetRelation()
            {
                var relationToUpdate = await _context.CustomerCompanyRelations.Where(
                        r => r.CustomerId == customerId && r.CompanyId == companyId && r.BranchId == branchId)
                    .SingleOrDefaultAsync();

                if (relationToUpdate == null)
                    return Result.Fail<CustomerCompanyRelation>("Customer not found in specified company or branch");

                return Result.Ok(relationToUpdate);
            }


            async Task<Result<List<InCompanyPermissions>>> UpdatePermissions(CustomerCompanyRelation relation)
            {
                var newPermissions = permissions.Aggregate((p1, p2) => p1 | p2);
                relation.InCompanyPermissions = newPermissions;

                _context.CustomerCompanyRelations.Update(relation);
                await _context.SaveChangesAsync();

                return Result.Ok(relation.InCompanyPermissions.ToList());
            }
        }


        private Result CheckCompanyAndBranch(CustomerInfo customer, int companyId, int branchId)
        {
            if (customer.CompanyId != companyId)
                return Result.Fail("The customer isn't affiliated with the company");

            // TODO When branch system gets ierarchic, this needs to be changed so that customer can see customers/markups of his own branch and its subbranches
            if (branchId != default && customer.BranchId != branchId)
                return Result.Fail("The customer isn't affiliated with the branch");

            return Result.Ok();
        }


        private async ValueTask<Result> Validate(CustomerRegistrationInfo customerRegistration, string externalIdentity)
        {
            var fieldValidateResult = GenericValidator<CustomerRegistrationInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Title).NotEmpty();
                v.RuleFor(c => c.FirstName).NotEmpty();
                v.RuleFor(c => c.LastName).NotEmpty();
            }, customerRegistration);

            if (fieldValidateResult.IsFailure)
                return fieldValidateResult;

            return await CheckIdentityIsUnique(externalIdentity);
        }


        private async Task<Result> CheckIdentityIsUnique(string identity)
        {
            return await _context.Customers.AnyAsync(c => c.IdentityHash == HashGenerator.ComputeSha256(identity))
                ? Result.Fail("User is already registered")
                : Result.Ok();
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICustomerContext _customerContext;
    }
}