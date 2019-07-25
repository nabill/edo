using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
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
                PreferredPaymentMethod = company.PreferredPaymentMethod,
                State = CompanyStates.PendingVerification
            };

            _context.Companies.Add(createdCompany);
            await _context.SaveChangesAsync();

            return Result.Ok(createdCompany);
        }

        private Result Validate(in CompanyRegistrationInfo companyRegistration)
        {
            return Result.Combine(
                CheckNotEmpty(companyRegistration.Name, nameof(companyRegistration.Name)),
                CheckNotEmpty(companyRegistration.Address, nameof(companyRegistration.Address)),
                CheckNotEmpty(companyRegistration.City, nameof(companyRegistration.City)),
                CheckNotEmpty(companyRegistration.CountryCode, nameof(companyRegistration.CountryCode)),
                CheckNotEmpty(companyRegistration.Phone, nameof(companyRegistration.Phone)));
        }

        private static Result CheckNotEmpty(string value, string propertyName)
        {
            return string.IsNullOrWhiteSpace(value) 
                ? Result.Fail($"Value of {propertyName} cannot be empty") 
                : Result.Ok();
        }
        
        private readonly EdoContext _context;
    }
}