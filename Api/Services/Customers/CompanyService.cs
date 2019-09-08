using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
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

        public async Task<Result<Company>> Create(CompanyRegistrationInfo company)
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
            return GenericValidator<CompanyRegistrationInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Name).NotEmpty();
                v.RuleFor(c => c.Address).NotEmpty();
                v.RuleFor(c => c.City).NotEmpty();
                v.RuleFor(c => c.Phone).NotEmpty().Matches(@"^[0-9]{3,30}$");
                v.RuleFor(c => c.Fax).Matches(@"^[0-9]{3,30}$");
            }, companyRegistration);
        }
        
        private readonly EdoContext _context;
    }
}