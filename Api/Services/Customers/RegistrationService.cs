using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;

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

        public async ValueTask<Result> RegisterMasterCustomer(CompanyRegistrationInfo company, CustomerRegistrationInfo masterCustomer)
        {
            var companyRegisterResult = await _companyService.Create(company);
            if (companyRegisterResult.IsFailure)
                return companyRegisterResult;
            
            var customerRegisterResult = await _customerService.Create(masterCustomer);
            if (customerRegisterResult.IsFailure)
                return customerRegisterResult;
            
            _context.CustomerCompanyRelations.Add(new CustomerCompanyRelation
            {
                CompanyId = companyRegisterResult.Value.Id,
                CustomerId = customerRegisterResult.Value.Id,
                Type = RelationType.Master
            });

            await _context.SaveChangesAsync();
            return Result.Ok();
        }
        
        private readonly ICompanyService _companyService;
        private readonly EdoContext _context;
        private readonly ICustomerService _customerService;
    }
}