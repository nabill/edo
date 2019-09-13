using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CompanyService : ICompanyService
    {
        public CompanyService(EdoContext context, 
            IAccountManagementService accountManagementService,
            IAdministratorContext administratorContext,
            IDateTimeProvider dateTimeProvider,
            IManagementAuditService managementAuditService)
        {
            _context = context;
            _accountManagementService = accountManagementService;
            _administratorContext = administratorContext;
            _dateTimeProvider = dateTimeProvider;
            _managementAuditService = managementAuditService;
        }

        public async Task<Result<Company>> Create(CompanyRegistrationInfo company)
        {
            var (_, isFailure, error) = Validate(company);
            if (isFailure)
                return Result.Fail<Company>(error);

            var now = _dateTimeProvider.UtcNow();
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
                State = CompanyStates.PendingVerification,
                Created = now,
                Updated = now
            };
            
            _context.Companies.Add(createdCompany);
            await _context.SaveChangesAsync();

            return Result.Ok(createdCompany);
        }

        public Task<Result> SetVerified(int companyId, string verifyReason)
        {
            return Result.Ok()
                .Ensure(HasVerifyRights, "Permission denied")
                .OnSuccess(GetCompany)
                .OnSuccessWithTransaction(_context, company => Result.Ok(company)
                    .OnSuccess(SetCompanyVerified)
                    .OnSuccess(CreatePaymentAccount)
                    .OnSuccess((WriteAuditLog)));
            
            Task<bool> HasVerifyRights()
            {
                return _administratorContext.HasGlobalPermission(GlobalPermissions.CompanyVerification);
            }
            
            async Task<Result<Company>> GetCompany()
            {
                var company = await _context.Companies.SingleOrDefaultAsync(c => c.Id == companyId);
                return company == default
                    ? Result.Fail<Company>($"Could not find company with id {companyId}")
                    : Result.Ok(company);
            }

            Task SetCompanyVerified(Company company)
            {
                company.State = CompanyStates.Verified;
                company.VerifyReason = verifyReason;
                var now = _dateTimeProvider.UtcNow();
                company.Verified = now;
                company.Updated = now;
                _context.Update(company);
                return _context.SaveChangesAsync();
            }
            
            Task<Result> CreatePaymentAccount(Company company)
            {
                return _accountManagementService
                    .CreateAccount(company, company.PreferredCurrency);
            }
            
            Task WriteAuditLog()
            {
                return _managementAuditService.Write(ManagementEventType.CompanyVerification, 
                    new CompanyVerifiedAuditEventData(companyId, verifyReason));
            }
        }

        private Result Validate(in CompanyRegistrationInfo companyRegistration)
        {
            return GenericValidator<CompanyRegistrationInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Name).NotEmpty();
                v.RuleFor(c => c.Address).NotEmpty();
                v.RuleFor(c => c.City).NotEmpty();
                v.RuleFor(c => c.Phone).NotEmpty().Matches(@"^[0-9]{3,30}$");
                v.RuleFor(c => c.Fax).Matches(@"^[0-9]{3,30}$").When(i => !string.IsNullOrWhiteSpace(i.Fax));
            }, companyRegistration);
        }
        
        private readonly EdoContext _context;
        private readonly IAccountManagementService _accountManagementService;
        private readonly IAdministratorContext _administratorContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IManagementAuditService _managementAuditService;
    }
}