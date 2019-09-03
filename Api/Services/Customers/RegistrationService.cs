using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class RegistrationService : IRegistrationService
    {
        public RegistrationService(EdoContext context, ICompanyService companyService,
            ICustomerService customerService)
        {
            _context = context;
            _companyService = companyService;
            _customerService = customerService;
        }

        public async ValueTask<Result> RegisterMasterCustomer(CompanyRegistrationInfo company, 
            CustomerRegistrationInfo masterCustomer,
            string externalIdentity)
        {
            var companyRegisterResult = await _companyService.Create(company);
            if (companyRegisterResult.IsFailure)
                return companyRegisterResult;
            
            var customerRegisterResult = await _customerService.Create(masterCustomer, externalIdentity);
            if (customerRegisterResult.IsFailure)
                return customerRegisterResult;
            
            _context.CustomerCompanyRelations.Add(new CustomerCompanyRelation
            {
                CompanyId = companyRegisterResult.Value.Id,
                CustomerId = customerRegisterResult.Value.Id,
                Type = CustomerCompanyRelationTypes.Master
            });

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> RegisterRegularCustomer(CustomerRegistrationInfo requestCustomerRegistrationInfo,
            int companyId, string externalIdentity)
        {
            return await _customerService.Create(requestCustomerRegistrationInfo, externalIdentity)
                .Ensure(RelationDoesNotExist, "Company relation is already exists")
                .Ensure(CompanyExists, "Company does not exist")
                .OnSuccess(AddCompanyRelation);

            async Task<bool> RelationDoesNotExist(Customer customer)
            {
                return !await _context.CustomerCompanyRelations
                    .Where(cr => cr.CustomerId == customer.Id)
                    .Where(cr => cr.CompanyId == companyId)
                    .AnyAsync();
            }
            
            Task<bool> CompanyExists(Customer customer)
            {
                return _context.Companies
                    .AnyAsync(c => c.Id == companyId);
            }

            Task<int> AddCompanyRelation(Customer customer)
            {
                _context.CustomerCompanyRelations.Add(new CustomerCompanyRelation
                {
                    Type = CustomerCompanyRelationTypes.Regular,
                    CompanyId = companyId,
                    CustomerId = customer.Id
                });
                return _context.SaveChangesAsync();
            }
        }

        private readonly ICompanyService _companyService;
        private readonly EdoContext _context;
        private readonly ICustomerService _customerService;
    }
}