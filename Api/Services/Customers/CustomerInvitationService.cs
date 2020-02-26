using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Customers;
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


        public async Task<Result> Send(CustomerInvitationInfo invitationInfo)
        {
            var (_, customerCompanyId, _, _) = await _customerContext.GetCustomer();

            if (customerCompanyId != invitationInfo.CompanyId)
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
        
        
        public async Task<Result<string>> Create(CustomerInvitationInfo invitationInfo)
        {
            var (_, customerCompanyId, _, _) = await _customerContext.GetCustomer();

            if (customerCompanyId != invitationInfo.CompanyId)
                return Result.Fail<string>("Invitations can be sent within a company only");
            
            return await _invitationService.Create(invitationInfo.Email, invitationInfo.RegistrationInfo, UserInvitationTypes.Customer);
        }


        public Task Accept(string invitationCode) => _invitationService.Accept(invitationCode);


        public Task<Result<CustomerInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _invitationService.GetPendingInvitation<CustomerInvitationInfo>(invitationCode, UserInvitationTypes.Customer);


        private readonly ICustomerContext _customerContext;
        private readonly IUserInvitationService _invitationService;
        private readonly CustomerInvitationOptions _options;
        private readonly IPermissionChecker _permissionChecker;
        private readonly EdoContext _context;
    }
}