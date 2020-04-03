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
            ICounterpartyService _counterpartyService,
            ICustomerService customerService,
            ICustomerInvitationService customerInvitationService,
            IOptions<CustomerRegistrationNotificationOptions> notificationOptions,
            IMailSender mailSender,
            ILogger<CustomerRegistrationService> logger)
        {
            _context = context;
            _counterpartyService = _counterpartyService;
            _customerService = customerService;
            _customerInvitationService = customerInvitationService;
            _notificationOptions = notificationOptions.Value;
            _mailSender = mailSender;
            _logger = logger;
        }


        public Task<Result> RegisterWithCounterparty(CustomerEditableInfo customerData, CounterpartyInfo counterpartyData, string externalIdentity,
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


            Task<Result<Company>> CreateCompany() => _counterpartyService.Add(counterpartyData);


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
                var defaultBranch = await _counterpartyService.GetDefaultBranch(company.Id);
                await AddCounterpartyRelation(customer,
                    company.Id,
                    CustomerCounterpartyRelationTypes.Master,
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
                    company = counterpartyData,
                    customerEmail = email,
                    customerName = customer
                };

                return await _mailSender.Send(_notificationOptions.MasterCustomerMailTemplateId, _notificationOptions.AdministratorsEmails, messageData);
            }


            Result LogSuccess((Company, Customer) registrationData)
            {
                var (company, customer) = registrationData;
                _logger.LogCustomerRegistrationSuccess($"Customer {customer.Email} with counterparty '{company.Name}' successfully registered");
                return Result.Ok();
            }


            void LogFailure(string error)
            {
                _logger.LogCustomerRegistrationFailed(error);
            }
        }


        public Task<Result> RegisterInvited(CustomerEditableInfo registrationInfo, string invitationCode, string externalIdentity, string email)
        {
            return Result.Ok()
                .Ensure(IsIdentityPresent, "User should have identity")
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


            async Task<CustomerInvitationInfo> AcceptInvitation((CustomerInvitationInfo invitationInfo, Customer customer, CounterpartyStates) invitationData)
            {
                await _customerInvitationService.Accept(invitationCode);
                return invitationData.invitationInfo;
            }


            async Task AddRegularCompanyRelation((CustomerInvitationInfo, Customer, CounterpartyStates) invitationData)
            {
                var (invitation, customer, state) = invitationData;
                
                //TODO: When we will able one customer account for different branches it will have different permissions, so add a branch check here
                var defaultBranch = await _counterpartyService.GetDefaultBranch(invitation.CounterpartyId);

                var permissions = state == CounterpartyStates.FullAccess
                    ? PermissionSets.FullAccessDefault
                    : PermissionSets.ReadOnlyDefault;

                await AddCounterpartyRelation(customer, invitation.CounterpartyId, CustomerCounterpartyRelationTypes.Regular, permissions, defaultBranch.Id);
            }


            async Task<Result<(CustomerInvitationInfo, Customer)>> CreateCustomer(CustomerInvitationInfo invitation)
            {
                var (_, isFailure, customer, error) = await _customerService.Add(registrationInfo, externalIdentity, email);
                return isFailure
                    ? Result.Fail<(CustomerInvitationInfo, Customer)>(error)
                    : Result.Ok((invitation, customer));
            }


            async Task<Result<(CustomerInvitationInfo, Customer, CounterpartyStates)>> GetCompanyState((CustomerInvitationInfo Info, Customer Customer) invitationData)
            {
                //TODO: When we will able one customer account for different branches it will have different permissions, so add a branch check here
                var state = await _context.Companies
                    .Where(c => c.Id == invitationData.Item1.CounterpartyId)
                    .Select(c => c.State)
                    .SingleOrDefaultAsync();

                return Result.Ok<(CustomerInvitationInfo, Customer, CounterpartyStates)>((invitationData.Info, invitationData.Customer, state));
            }


            Task<Result<Customer>> GetMasterCustomer(CustomerInvitationInfo invitationInfo) => _customerService.GetMasterCustomer(invitationInfo.CounterpartyId);


            Task<Result<CustomerInvitationInfo>> GetPendingInvitation() => _customerInvitationService.GetPendingInvitation(invitationCode);


            bool IsIdentityPresent() => !string.IsNullOrWhiteSpace(externalIdentity);


            void LogFailed(string error)
            {
                _logger.LogCustomerRegistrationFailed(error);
            }


            Result<CustomerInvitationInfo> LogSuccess(CustomerInvitationInfo invitationInfo)
            {
                _logger.LogCustomerRegistrationSuccess($"Customer {email} successfully registered and bound to counterparty ID:'{invitationInfo.CounterpartyId}'");
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


        private Task AddCounterpartyRelation(Customer customer, int companyId, CustomerCounterpartyRelationTypes relationType, InCounterpartyPermissions permissions, int branchId)
        {
            _context.CustomerCompanyRelations.Add(new CustomerCompanyRelation
            {
                CompanyId = companyId,
                CustomerId = customer.Id,
                Type = relationType,
                InCounterpartyPermissions = permissions,
                BranchId = branchId
            });

            return _context.SaveChangesAsync();
        }


        private readonly ICounterpartyService _counterpartyService;

        private readonly EdoContext _context;
        private readonly ICustomerInvitationService _customerInvitationService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerRegistrationService> _logger;
        private readonly IMailSender _mailSender;
        private readonly CustomerRegistrationNotificationOptions _notificationOptions;
    }
}