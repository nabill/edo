using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        public CustomerService(EdoContext context, IDateTimeProvider dateTimeProvider, ICustomerContext customerContext,
            IMarkupPolicyTemplateService markupPolicyTemplateService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _customerContext = customerContext;
            _markupPolicyTemplateService = markupPolicyTemplateService;
        }


        public async Task<Result<Customer>> Add(CustomerEditableInfo customerRegistration,
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


        public async Task<CustomerEditableInfo> UpdateCurrentCustomer(CustomerEditableInfo newInfo)
        {
            var currentCustomerInfo = await _customerContext.GetCustomer();
            var customerToUpdate = await _context.Customers.SingleAsync(c => c.Id == currentCustomerInfo.CustomerId);

            customerToUpdate.FirstName = newInfo.FirstName;
            customerToUpdate.LastName = newInfo.LastName;
            customerToUpdate.Title = newInfo.Title;
            customerToUpdate.Position = newInfo.Position;

            _context.Customers.Update(customerToUpdate);
            await _context.SaveChangesAsync();

            return newInfo;
        }

        public async Task<Result<List<SlimCustomerInfo>>> GetCustomers(int companyId, int branchId = default)
        {
            var currentCustomer = await _customerContext.GetCustomer();
            var (_, isFailure, error) = CheckCompanyAndBranch(currentCustomer, companyId, branchId);
            if (isFailure)
                return Result.Fail<List<SlimCustomerInfo>>(error);

            var relations = await
                (from relation in _context.CustomerCompanyRelations
                join customer in _context.Customers
                    on relation.CustomerId equals customer.Id
                join company in _context.Companies
                    on relation.CompanyId equals company.Id
                join branch in _context.Branches
                    on relation.BranchId equals branch.Id
                 where branchId == default ? relation.CompanyId == companyId : relation.BranchId == branchId
                 select new {relation, customer, company, branch})
                .ToListAsync();

            var customerIdList = relations.Select(x => x.customer.Id).ToList();

            var markupsMap = (await (
                from markup in _context.MarkupPolicies
                where markup.CustomerId != null 
                    && customerIdList.Contains(markup.CustomerId.Value)
                    && markup.ScopeType == MarkupPolicyScopeType.Customer
                select markup)
                .ToListAsync())
                .GroupBy(k => (int)k.CustomerId)
                .ToDictionary(k => k.Key, v => v.ToList());

            var results = relations.Select(o => 
                new SlimCustomerInfo(o.customer.Id, o.customer.FirstName, o.customer.LastName,
                    o.customer.Created, o.company.Id, o.company.Name, o.branch.Id, o.branch.Title,
                    GetMarkupFormula(o.relation)))
                .ToList();

            return Result.Ok(results);

            string GetMarkupFormula(CustomerCompanyRelation relation)
            {
                if (!markupsMap.TryGetValue(relation.CustomerId, out var policies))
                    return string.Empty;
                
                // TODO this needs to be reworked once branches become ierarchic
                if (currentCustomer.InCompanyPermissions.HasFlag(InCompanyPermissions.ObserveMarkupInCompany)
                    || currentCustomer.InCompanyPermissions.HasFlag(InCompanyPermissions.ObserveMarkupInBranch) && relation.BranchId == branchId)
                    return _markupPolicyTemplateService.GetMarkupsFormula(policies);

                return string.Empty;
            }
        }


        public async Task<Result<CustomerInfoInBranch>> GetCustomer(int companyId, int branchId, int customerId)
        {
            var customer = await _customerContext.GetCustomer();
            var (_, isFailure, error) = CheckCompanyAndBranch(customer, companyId, branchId);
            if (isFailure)
                return Result.Fail<CustomerInfoInBranch>(error);

            // TODO this needs to be reworked when customers will be able to belong to more than one branch within a company
            var foundCustomer = await (
                    from cr in _context.CustomerCompanyRelations
                    join c in _context.Customers
                        on cr.CustomerId equals c.Id
                    join co in _context.Companies
                        on cr.CompanyId equals co.Id
                    join br in _context.Branches
                        on cr.BranchId equals br.Id
                    where (branchId == default ? cr.CompanyId == companyId : cr.BranchId == branchId)
                        && cr.CustomerId == customerId
                    select (CustomerInfoInBranch?) new CustomerInfoInBranch(c.Id, c.FirstName, c.LastName, c.Email, c.Title, c.Position, co.Id, co.Name,
                        cr.BranchId, br.Title, cr.Type == CustomerCompanyRelationTypes.Master, cr.InCompanyPermissions.ToList()))
                .SingleOrDefaultAsync();

            if (foundCustomer == null)
                return Result.Fail<CustomerInfoInBranch>("Customer not found in specified company or branch");

            return Result.Ok(foundCustomer.Value);
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


        private async ValueTask<Result> Validate(CustomerEditableInfo customerRegistration, string externalIdentity)
        {
            var fieldValidateResult = GenericValidator<CustomerEditableInfo>.Validate(v =>
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
        private readonly IMarkupPolicyTemplateService _markupPolicyTemplateService;
    }
}