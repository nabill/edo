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
                .Ensure(IsRelationExists, "Company relations is already exist")
                .Ensure(IsNotExistingCompany, "Company does not exist")
                .OnSuccess(AddCompanyRelation)
                .OnFailure(RemoveEntities);

            Task<bool> IsRelationExists(Customer customer)
            {
                return _context.CustomerCompanyRelations
                    .Where(cr => cr.CustomerId == customer.Id)
                    .Where(cr => cr.CompanyId == companyId)
                    .AnyAsync();
            }
            
            async Task<bool> IsNotExistingCompany(Customer customer)
            {
                return !await _context.Companies
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

            async Task RemoveEntities()
            {
                var customer = await _context.Customers.SingleOrDefaultAsync(c => c.Email == requestCustomerRegistrationInfo.Email);
                if (customer != null)
                {
                    _context.Customers.Remove(customer);
                    var companyRelation = await _context.CustomerCompanyRelations
                        .Where(cr => cr.CompanyId == companyId)
                        .Where(cr => cr.CustomerId == customer.Id)
                        .SingleOrDefaultAsync();

                    if (!(companyRelation is null))
                        _context.CustomerCompanyRelations.Remove(companyRelation);

                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private readonly ICompanyService _companyService;
        private readonly EdoContext _context;
        private readonly ICustomerService _customerService;
    }
}