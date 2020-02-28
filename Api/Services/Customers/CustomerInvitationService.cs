using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerInvitationService : ICustomerInvitationService
    {
        public CustomerInvitationService(ICustomerContext customerContext,
            IOptions<CustomerInvitationOptions> options,
            IUserInvitationService invitationService,
            ICompanyService companyService)
        {
            _customerContext = customerContext;
            _invitationService = invitationService;
            _companyService = companyService;
            _options = options.Value;
        }


        public async Task<Result> Send(CustomerInvitationInfo invitationInfo)
        {
            var customerCompanyId = (await _customerContext.GetCustomer()).CompanyId;

            if (customerCompanyId != invitationInfo.CompanyId)
                return Result.Fail("Invitations can be send within a company only");

            var companyName = (await _companyService.Get(customerCompanyId)).Value.Name;
            
            var messagePayloadGenerator = new Func<CustomerInvitationInfo, string, object>((info, invitationCode) => new
            {
                companyName,
                invitationCode,
                userEmailAddress = info.Email,
                userName = $"{info.RegistrationInfo.FirstName} {info.RegistrationInfo.LastName}"
            });

            return await _invitationService.Send(invitationInfo.Email, invitationInfo, messagePayloadGenerator,
                _options.MailTemplateId, UserInvitationTypes.Customer);
        }
        
        
        public async Task<Result<string>> Create(CustomerInvitationInfo invitationInfo)
        {
            var (_, customerCompanyId, _, _) = await _customerContext.GetCustomer();

            if (customerCompanyId != invitationInfo.CompanyId)
                return Result.Fail<string>("Invitations can be send within a company only");
            
            return await _invitationService.Create(invitationInfo.Email, invitationInfo.RegistrationInfo, UserInvitationTypes.Customer);
        }


        public Task Accept(string invitationCode) => _invitationService.Accept(invitationCode);


        public Task<Result<CustomerInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _invitationService.GetPendingInvitation<CustomerInvitationInfo>(invitationCode, UserInvitationTypes.Customer);


        private readonly ICustomerContext _customerContext;
        private readonly IUserInvitationService _invitationService;
        private readonly ICompanyService _companyService;
        private readonly CustomerInvitationOptions _options;
    }
}