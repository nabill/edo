using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerInvitationService : ICustomerInvitationService
    {
        public CustomerInvitationService(ICustomerContext customerContext,
            IOptions<CustomerInvitationOptions> options,
            IUserInvitationService invitationService)
        {
            _customerContext = customerContext;
            _invitationService = invitationService;
            _options = options.Value;
        }
        
        public async Task<Result> SendInvitation(CustomerInvitationInfo invitationInfo)
        {
            // TODO: move to authorization policies.
            if(!await _customerContext.IsMasterCustomer())
                return Result.Fail("Only master customers can send invitations");

            return await _invitationService.SendInvitation(invitationInfo.Email, invitationInfo,
                _options.MailTemplateId);
        }

        public Task AcceptInvitation(string invitationCode)
        {
            return _invitationService.AcceptInvitation(invitationCode);
        }

        public Task<Result<CustomerInvitationInfo>> GetPendingInvitation(string invitationCode)
        {
            return _invitationService.GetPendingInvitation<CustomerInvitationInfo>(invitationCode);
        }
        
        private readonly ICustomerContext _customerContext;
        private readonly IUserInvitationService _invitationService;
        private readonly CustomerInvitationOptions _options;
    }
}