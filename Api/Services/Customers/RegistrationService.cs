using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Companies;
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
            var companyRegisterResult = _companyService.Create(company);
            if (companyRegisterResult.IsFailure)
                return Result.Fail(companyRegisterResult.Error);

            var customerRegisterResult = await _customerService.Create(masterCustomer);
            if (customerRegisterResult.IsFailure)
                return Result.Fail(customerRegisterResult.Error);

            _context.CustomerCompanyRelations.Add(new CustomerCompanyRelation
            {
                Company = companyRegisterResult.Value,
                Customer = customerRegisterResult.Value,
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