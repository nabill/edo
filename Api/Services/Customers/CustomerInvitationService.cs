using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerInvitationService : ICustomerInvitationService
    {
        public CustomerInvitationService(ICustomerContext customerContext,
            IOptions<CustomerInvitationOptions> options,
            IUserInvitationService invitationService,
            IPermissionChecker permissionChecker,
            EdoContext context)
        {
            _customerContext = customerContext;
            _invitationService = invitationService;
            _permissionChecker = permissionChecker;
            _context = context;
            _options = options.Value;
        }


        public async Task<Result> SendInvitation(CustomerInvitationInfo invitationInfo)
        {
            var (_, isFailure, customerInfo, error) = await _customerContext.GetCustomerInfo();
            if (isFailure)
                return Result.Fail(error);

            if (customerInfo.CompanyId != invitationInfo.CompanyId)
                return Result.Fail("Invitations can be sent within a company only");

            var companyName = await _context.Companies
                    .Where(c => c.Id == invitationInfo.CompanyId)
                    .Select(c => c.Name)
                    .SingleAsync();
            
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


        public Task AcceptInvitation(string invitationCode) => _invitationService.Accept(invitationCode);


        public Task<Result<CustomerInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _invitationService.GetPendingInvitation<CustomerInvitationInfo>(invitationCode, UserInvitationTypes.Customer);


        private readonly ICustomerContext _customerContext;
        private readonly IUserInvitationService _invitationService;
        private readonly CustomerInvitationOptions _options;
        private readonly IPermissionChecker _permissionChecker;
        private readonly EdoContext _context;
    }
}