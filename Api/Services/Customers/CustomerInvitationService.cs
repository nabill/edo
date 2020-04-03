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
            ICounterpartyService counterpartyService)
        {
            _customerContext = customerContext;
            _invitationService = invitationService;
            _counterpartyService = counterpartyService;
            _options = options.Value;
        }


        public async Task<Result> Send(CustomerInvitationInfo invitationInfo)
        {
            var customerCounterpartyId = (await _customerContext.GetCustomer()).CounterpartyId;

            if (customerCounterpartyId != invitationInfo.CounterpartyId)
                return Result.Fail("Invitations can be send within a counterparty only");

            var counterpartyName = (await _counterpartyService.Get(customerCounterpartyId)).Value.Name;
            
            var messagePayloadGenerator = new Func<CustomerInvitationInfo, string, object>((info, invitationCode) => new
            {
                counterpartyName,
                invitationCode,
                userEmailAddress = info.Email,
                userName = $"{info.RegistrationInfo.FirstName} {info.RegistrationInfo.LastName}"
            });

            return await _invitationService.Send(invitationInfo.Email, invitationInfo, messagePayloadGenerator,
                _options.MailTemplateId, UserInvitationTypes.Customer);
        }
        
        
        public async Task<Result<string>> Create(CustomerInvitationInfo invitationInfo)
        {
            var (_, customerCounterpartyId, _, _) = await _customerContext.GetCustomer();

            if (customerCounterpartyId != invitationInfo.CounterpartyId)
                return Result.Fail<string>("Invitations can be send within a counterparty only");
            
            return await _invitationService.Create(invitationInfo.Email, invitationInfo, UserInvitationTypes.Customer);
        }


        public Task Accept(string invitationCode) => _invitationService.Accept(invitationCode);


        public Task<Result<CustomerInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _invitationService.GetPendingInvitation<CustomerInvitationInfo>(invitationCode, UserInvitationTypes.Customer);


        private readonly ICustomerContext _customerContext;
        private readonly IUserInvitationService _invitationService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly CustomerInvitationOptions _options;
    }
}