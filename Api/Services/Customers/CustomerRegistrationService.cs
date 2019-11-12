using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerRegistrationService : ICustomerRegistrationService
    {
        public CustomerRegistrationService(EdoContext context,
            ICompanyService companyService,
            ICustomerService customerService,
            ICustomerInvitationService customerInvitationService,
            ILogger<CustomerRegistrationService> logger)
        {
            _context = context;
            _companyService = companyService;
            _customerService = customerService;
            _customerInvitationService = customerInvitationService;
            _logger = logger;
        }

        public Task<Result> RegisterWithCompany(CustomerRegistrationInfo customerData,
            CompanyRegistrationInfo companyData, 
            string externalIdentity,
            string email)
        {
            return Result.Ok()
                .Ensure(IdentityIsPresent, "User should have identity")
                .OnSuccessWithTransaction(_context, () => Result.Ok()
                    .OnSuccess(CreateCompany)
                    .OnSuccess(CreateCustomer)
                    .OnSuccess(AddMasterCompanyRelation))
                .OnSuccess(LogSuccess)
                .OnFailure(LogFailure);
            
            bool IdentityIsPresent()
            {
                return !string.IsNullOrWhiteSpace(externalIdentity);
            }
            
            Task<Result<Company>> CreateCompany()
            {
                return _companyService.Add(companyData);
            }
            
            async Task<Result<(Company, Customer)>> CreateCustomer(Company company)
            {
                var (_, isFailure, customer, error) = await _customerService.Add(customerData, externalIdentity, email);
                return isFailure
                    ? Result.Fail<(Company, Customer)>(error)
                    : Result.Ok((company1: company, customer));
            }

            Task AddMasterCompanyRelation((Company company, Customer customer) companyUserInfo)
            {
                return AddCompanyRelation(companyUserInfo.customer,
                    companyUserInfo.company.Id,
                    CustomerCompanyRelationTypes.Master,
                    InCompanyPermissions.All);
            }
            
            Result LogSuccess((Company, Customer) registrationData)
            {
                var (company, customer) = registrationData;
                _logger.LogCustomerRegistrationSuccess($"Customer {customer.Email} with company {company.Name} successfully registered");
                return Result.Ok();
            }
            
            void LogFailure(string error)
            {
                _logger.LogCustomerRegistrationFailed(error);
            }
        }

        public Task<Result> RegisterInvited(CustomerRegistrationInfo registrationInfo,
            string invitationCode, string externalIdentity, string email)
        {
            return Result.Ok()
                .Ensure(IdentityIsPresent, "User should have identity")
                .OnSuccess(GetPendingInvitation)
                .OnSuccessWithTransaction( _context, invitation => Result.Ok(invitation) 
                    .OnSuccess(CreateCustomer)
                    .OnSuccess(AddRegularCompanyRelation)
                    .OnSuccess(AcceptInvitation))
                .OnSuccess(LogSuccess)
                .OnFailure(LogFailed);
            
            bool IdentityIsPresent()
            {
                return !string.IsNullOrWhiteSpace(externalIdentity);
            }
            
            Task<Result<CustomerInvitationInfo>> GetPendingInvitation()
            {
                return _customerInvitationService.GetPendingInvitation(invitationCode);
            }
            
            async Task<Result<(CustomerInvitationInfo, Customer)>> CreateCustomer(CustomerInvitationInfo invitation)
            {
                var (_, isFailure, customer, error) = await _customerService.Add(registrationInfo, externalIdentity, email);
                return isFailure
                    ? Result.Fail<(CustomerInvitationInfo, Customer)>(error)
                    : Result.Ok((invitation, customer));
            }
            
            Task AddRegularCompanyRelation((CustomerInvitationInfo invitation, Customer customer) invitationData)
            {
                return AddCompanyRelation(invitationData.customer,
                    invitationData.invitation.CompanyId,
                    CustomerCompanyRelationTypes.Regular,
                    DefaultCustomerPermissions);
            }
            
            async Task<(CustomerInvitationInfo invitationInfo, Customer customer)> AcceptInvitation((CustomerInvitationInfo invitationInfo, Customer customer) invitationData)
            {
                await _customerInvitationService.AcceptInvitation(invitationCode);
                return invitationData;
            }
            
            Result LogSuccess((CustomerInvitationInfo, Customer) registrationData)
            {
                var (invitation, customer) = registrationData;
                _logger.LogCustomerRegistrationSuccess($"Customer {customer.Email} successfully registered and bound to company ID:'{invitation.CompanyId}'");
                return Result.Ok();
            }
            
            void LogFailed(string error)
            {
                _logger.LogCustomerRegistrationFailed(error);
            }
        }

        private Task AddCompanyRelation(Customer customer, int companyId, CustomerCompanyRelationTypes relationType, InCompanyPermissions permissions)
        {
            _context.CustomerCompanyRelations.Add(new CustomerCompanyRelation
            {
                CompanyId = companyId,
                CustomerId = customer.Id,
                Type = relationType,
                InCompanyPermissions = permissions
            });

            return _context.SaveChangesAsync();
        }
        
        private const InCompanyPermissions DefaultCustomerPermissions = InCompanyPermissions.AccommodationAvailabilitySearch |
            InCompanyPermissions.AccommodationBooking;
            
        private readonly EdoContext _context;
        private readonly ICompanyService _companyService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerInvitationService _customerInvitationService;
        private readonly ILogger<CustomerRegistrationService> _logger;
    }
}