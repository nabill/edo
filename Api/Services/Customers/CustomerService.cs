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


        public async Task<Result<Customer>> GetMasterCustomer(int counterpartyId)
        {
            var master = await (from c in _context.Customers
                join rel in _context.CustomerCounterpartyRelations on c.Id equals rel.CustomerId
                where rel.CounterpartyId == counterpartyId && rel.Type == CustomerCounterpartyRelationTypes.Master
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

        public async Task<Result<List<SlimCustomerInfo>>> GetCustomers(int counterpartyId, int agencyId = default)
        {
            var currentCustomer = await _customerContext.GetCustomer();
            var (_, isFailure, error) = CheckCounterpartyAndAgency(currentCustomer, counterpartyId, agencyId);
            if (isFailure)
                return Result.Fail<List<SlimCustomerInfo>>(error);

            var relations = await
                (from relation in _context.CustomerCounterpartyRelations
                join customer in _context.Customers
                    on relation.CustomerId equals customer.Id
                join counterparty in _context.Counterparties
                    on relation.CounterpartyId equals counterparty.Id
                join agency in _context.Agencies
                    on relation.AgencyId equals agency.Id
                 where agencyId == default ? relation.CounterpartyId == counterpartyId : relation.AgencyId == agencyId
                 select new {relation, customer, counterparty, agency})
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
                    o.customer.Created, o.counterparty.Id, o.counterparty.Name, o.agency.Id, o.agency.Title,
                    GetMarkupFormula(o.relation)))
                .ToList();

            return Result.Ok(results);

            string GetMarkupFormula(CustomerCounterpartyRelation relation)
            {
                if (!markupsMap.TryGetValue(relation.CustomerId, out var policies))
                    return string.Empty;
                
                // TODO this needs to be reworked once agencies become ierarchic
                if (currentCustomer.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.ObserveMarkupInCounterparty)
                    || currentCustomer.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.ObserveMarkupInAgency) && relation.AgencyId == agencyId)
                    return _markupPolicyTemplateService.GetMarkupsFormula(policies);

                return string.Empty;
            }
        }


        public async Task<Result<CustomerInfoInAgency>> GetCustomer(int counterpartyId, int agencyId, int customerId)
        {
            var customer = await _customerContext.GetCustomer();
            var (_, isFailure, error) = CheckCounterpartyAndAgency(customer, counterpartyId, agencyId);
            if (isFailure)
                return Result.Fail<CustomerInfoInAgency>(error);

            // TODO this needs to be reworked when customers will be able to belong to more than one agency within a counterparty
            var foundCustomer = await (
                    from cr in _context.CustomerCounterpartyRelations
                    join c in _context.Customers
                        on cr.CustomerId equals c.Id
                    join co in _context.Counterparties
                        on cr.CounterpartyId equals co.Id
                    join br in _context.Agencies
                        on cr.AgencyId equals br.Id
                    where (agencyId == default ? cr.CounterpartyId == counterpartyId : cr.AgencyId == agencyId)
                        && cr.CustomerId == customerId
                    select (CustomerInfoInAgency?) new CustomerInfoInAgency(c.Id, c.FirstName, c.LastName, c.Email, c.Title, c.Position, co.Id, co.Name,
                        cr.AgencyId, br.Title, cr.Type == CustomerCounterpartyRelationTypes.Master, cr.InCounterpartyPermissions.ToList()))
                .SingleOrDefaultAsync();

            if (foundCustomer == null)
                return Result.Fail<CustomerInfoInAgency>("Customer not found in specified counterparty or agency");

            return Result.Ok(foundCustomer.Value);
        }


        private Result CheckCounterpartyAndAgency(CustomerInfo customer, int counterpartyId, int agencyId)
        {
            if (customer.CounterpartyId != counterpartyId)
                return Result.Fail("The customer isn't affiliated with the counterparty");

            // TODO When agency system gets ierarchic, this needs to be changed so that customer can see customers/markups of his own agency and its subagencies
            if (agencyId != default && customer.AgencyId != agencyId)
                return Result.Fail("The customer isn't affiliated with the agency");

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