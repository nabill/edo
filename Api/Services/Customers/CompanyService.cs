using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CompanyService : ICompanyService
    {
        public CompanyService(EdoContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Company>> Create(CompanyRegistrationInfo company)
        {
            var (_, isFailure, error) = Validate(company);
            if (isFailure)
                return Result.Fail<Company>(error);

            var createdCompany = new Company
            {
                Address = company.Address,
                City = company.City,
                CountryCode = company.CountryCode,
                Fax = company.Fax,
                Name = company.Name,
                Phone = company.Phone,
                Website = company.Website,
                PostalCode = company.PostalCode,
                PreferredCurrency = company.PreferredCurrency,
                PreferredPaymentMethod = company.PreferredPaymentMethod
            };

            _context.Companies.Add(createdCompany);
            await _context.SaveChangesAsync();

            return Result.Ok(createdCompany);
        }

        private Result Validate(in CompanyRegistrationInfo companyRegistration)
        {
            // TODO: company validation
            return Result.Ok();
        }
        
        private readonly EdoContext _context;
    }
}