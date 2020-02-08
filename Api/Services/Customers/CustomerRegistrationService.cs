using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.MailSender;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerRegistrationService : ICustomerRegistrationService
    {
        public CustomerRegistrationService(EdoContext context,
            ICompanyService companyService,
            ICustomerService customerService,
            ICustomerInvitationService customerInvitationService,
            IOptions<CustomerRegistrationNotificationOptions> notificationOptions,
            IMailSender mailSender,
            ILogger<CustomerRegistrationService> logger)
        {
            _context = context;
            _companyService = companyService;
            _customerService = customerService;
            _customerInvitationService = customerInvitationService;
            _notificationOptions = notificationOptions.Value;
            _mailSender = mailSender;
            _logger = logger;
        }


        public Task<Result> RegisterWithCompany(CustomerRegistrationInfo customerData, CompanyRegistrationInfo companyData, string externalIdentity,
            string email)
        {
            return Result.Ok()
                .Ensure(IsIdentityPresent, "User should have identity")
                .OnSuccessWithTransaction(_context, () => Result.Ok()
                    .OnSuccess(CreateCompany)
                    .OnSuccess(CreateCustomer)
                    .OnSuccess(AddMasterCompanyRelation))
                .OnSuccess(LogSuccess)
                .OnSuccess(SendRegistrationMailToAdmins)
                .OnFailure(LogFailure);


            bool IsIdentityPresent() => !string.IsNullOrWhiteSpace(externalIdentity);


            Task<Result<Company>> CreateCompany() => _companyService.Add(companyData);


            async Task<Result<(Company, Customer)>> CreateCustomer(Company company)
            {
                var (_, isFailure, customer, error) = await _customerService.Add(customerData, externalIdentity, email);
                return isFailure
                    ? Result.Fail<(Company, Customer)>(error)
                    : Result.Ok((company1: company, customer));
            }


            async Task AddMasterCompanyRelation((Company company, Customer customer) companyUserInfo)
            {
                var (company, customer) = companyUserInfo;
                var defaultBranch = await _companyService.GetDefaultBranch(company.Id);
                await AddCompanyRelation(customer,
                    company.Id,
                    CustomerCompanyRelationTypes.Master,
                    PermissionSets.ReadOnlyMaster,
                    defaultBranch.Id);
            }


            async Task<Result> SendRegistrationMailToAdmins()
            {
                var customer = $"{customerData.Title} {customerData.FirstName} {customerData.LastName}";
                if (!string.IsNullOrWhiteSpace(customerData.Position))
                    customer += $" ({customerData.Position})";

                var messageData = new
                {
                    company = companyData,
                    customerName = customer
                };

                return await _mailSender.Send(_notificationOptions.MasterCustomerMailTemplateId, _notificationOptions.AdministratorsEmails, messageData);
            }


            Result LogSuccess((Company, Customer) registrationData)
            {
                var (company, customer) = registrationData;
                _logger.LogCustomerRegistrationSuccess($"Customer {customer.Email} with company '{company.Name}' successfully registered");
                return Result.Ok();
            }


            void LogFailure(string error)
            {
                _logger.LogCustomerRegistrationFailed(error);
            }
        }


        public Task<Result> RegisterInvited(CustomerRegistrationInfo registrationInfo, string invitationCode, string externalIdentity, string email)
        {
            return Result.Ok()
                .Ensure(IsIdentityIsPresent, "User should have identity")
                .OnSuccess(GetPendingInvitation)
                .OnSuccessWithTransaction(_context, invitation => Result.Ok(invitation)
                    .OnSuccess(CreateCustomer)
                    .OnSuccess(GetCompanyState)
                    .OnSuccess(AddRegularCompanyRelation)
                    .OnSuccess(AcceptInvitation))
                .OnSuccess(LogSuccess)
                .OnSuccess(GetMasterCustomer)
                .OnSuccess(SendRegistrationMailToMaster)
                .OnFailure(LogFailed);


            async Task<CustomerInvitationInfo> AcceptInvitation((CustomerInvitationInfo invitationInfo, Customer customer, CompanyStates) invitationData)
            {
                await _customerInvitationService.AcceptInvitation(invitationCode);
                return invitationData.invitationInfo;
            }


            async Task AddRegularCompanyRelation((CustomerInvitationInfo, Customer, CompanyStates) invitationData)
            {
                var (invitation, customer, state) = invitationData;
                
                //TODO: add a branch check here
                var defaultBranch = await _companyService.GetDefaultBranch(invitation.CompanyId);

                var permissions = state == CompanyStates.FullAccess
                    ? PermissionSets.FullAccessDefault
                    : PermissionSets.ReadOnlyDefault;

                await AddCompanyRelation(customer, invitation.CompanyId, CustomerCompanyRelationTypes.Regular, permissions, defaultBranch.Id);
            }


            async Task<Result<(CustomerInvitationInfo, Customer)>> CreateCustomer(CustomerInvitationInfo invitation)
            {
                var (_, isFailure, customer, error) = await _customerService.Add(registrationInfo, externalIdentity, email);
                return isFailure
                    ? Result.Fail<(CustomerInvitationInfo, Customer)>(error)
                    : Result.Ok((invitation, customer));
            }


            async Task<Result<(CustomerInvitationInfo, Customer, CompanyStates)>> GetCompanyState((CustomerInvitationInfo, Customer) invitationData)
            {
                //TODO: add a branch check here
                var state = await _context.Companies
                    .Where(c => c.Id == invitationData.Item1.CompanyId)
                    .Select(c => c.State)
                    .SingleOrDefaultAsync();

                return Result.Ok<(CustomerInvitationInfo, Customer, CompanyStates)>((invitationData.Item1, invitationData.Item2, state));
            }


            Task<Result<Customer>> GetMasterCustomer(CustomerInvitationInfo invitationInfo) => _customerService.GetMasterCustomer(invitationInfo.CompanyId);


            Task<Result<CustomerInvitationInfo>> GetPendingInvitation() => _customerInvitationService.GetPendingInvitation(invitationCode);


            bool IsIdentityIsPresent() => !string.IsNullOrWhiteSpace(externalIdentity);


            void LogFailed(string error)
            {
                _logger.LogCustomerRegistrationFailed(error);
            }


            Result<CustomerInvitationInfo> LogSuccess(CustomerInvitationInfo invitationInfo)
            {
                _logger.LogCustomerRegistrationSuccess($"Customer {email} successfully registered and bound to company ID:'{invitationInfo.CompanyId}'");
                return Result.Ok(invitationInfo);
            }


            async Task<Result> SendRegistrationMailToMaster(Customer master)
            {
                var position = registrationInfo.Position;
                if (string.IsNullOrWhiteSpace(position))
                    position = "a new employee";

                var (_, isFailure, error) = await _mailSender.Send(_notificationOptions.RegularCustomerMailTemplateId, master.Email, new
                {
                    customerName = $"{registrationInfo.FirstName} {registrationInfo.LastName}",
                    position,
                    title = registrationInfo.Title
                });
                if (isFailure)
                    return Result.Fail(error);

                return Result.Ok();
            }
        }


        private Task AddCompanyRelation(Customer customer, int companyId, CustomerCompanyRelationTypes relationType, InCompanyPermissions permissions, int branchId)
        {
            _context.CustomerCompanyRelations.Add(new CustomerCompanyRelation
            {
                CompanyId = companyId,
                CustomerId = customer.Id,
                Type = relationType,
                InCompanyPermissions = permissions,
                BranchId = branchId
            });

            return _context.SaveChangesAsync();
        }


        private readonly ICompanyService _companyService;

        private readonly EdoContext _context;
        private readonly ICustomerInvitationService _customerInvitationService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerRegistrationService> _logger;
        private readonly IMailSender _mailSender;
        private readonly CustomerRegistrationNotificationOptions _notificationOptions;
    }
}