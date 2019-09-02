using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Email;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class InvitationService : IInvitationService
    {
        public InvitationService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            ITemplatedMailSender mailSender,
            IRegistrationService registrationService,
            ICustomerContext customerContext,
            IOptions<InvitationOptions> options)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _mailSender = mailSender;
            _registrationService = registrationService;
            _customerContext = customerContext;
            _options = options.Value;
        }
        
        public async Task<Result> SendInvitation(RegularCustomerInvitation registrationInfo)
        {
            // TODO: move to authorization policies.
            if(!await _customerContext.IsMasterCustomer())
                return Result.Fail("Only master customers can send invitations");
            
            var invitationCode = HashGenerator.ComputeHash(Guid.NewGuid().ToString());
            var addresseeEmail = registrationInfo.RegistrationInfo.Email;
            
            await _mailSender.Send(templateId: _options.MailTemplateId,
                emailTo: new EmailAddress(addresseeEmail), 
                messageData: new InvitationData { InvitationCode = invitationCode });
            
            _context.CustomerInvitations.Add(new CustomerInvitation
            {
                Code = invitationCode,
                Created = _dateTimeProvider.UtcNow(),
                Data = JsonConvert.SerializeObject(registrationInfo),
                Email = addresseeEmail
            });

            await _context.SaveChangesAsync();
            return Result.Ok();
        }
        
        public async Task<Result> AcceptInvitation(CustomerRegistrationInfo customerRegistrationInfo, 
            string invitationCode, string identity)
        {
            var existingInvitation = await _context.CustomerInvitations
                .SingleOrDefaultAsync(c => c.Code == invitationCode);
            
            return await Result.Ok(existingInvitation)
                .Ensure(invitation => !(invitation is null), "Invalid invitation code")
                .Ensure(invitation => !invitation.IsAccepted, "Invitation is already accepted")
                .Ensure(InvitationIsActual, "Invitation expired")
                .OnSuccess(RegisterRegularCustomer)
                .OnSuccess(SetInvitationAccepted);

            Task<Result> RegisterRegularCustomer(CustomerInvitation invitation)
            {
                var invitationData = GetInvitationData(invitation);
                return _registrationService
                    .RegisterRegularCustomer(customerRegistrationInfo, invitationData.CompanyId, identity);
            }

            Task<int> SetInvitationAccepted()
            {
                existingInvitation.IsAccepted = true;
                _context.Update(existingInvitation);
                return _context.SaveChangesAsync();
            }

            bool InvitationIsActual(CustomerInvitation invitation)
            {
                return invitation.Created + _options.InvitationExpirationPeriod > _dateTimeProvider.UtcNow();
            }
        }

        public async Task<Result<CustomerRegistrationInfo>> GetInvitationInfo(string invitationCode)
        {
            var invitation = await _context.CustomerInvitations
                .SingleOrDefaultAsync(c => c.Code == invitationCode);

            return invitation is null
                ? Result.Fail<CustomerRegistrationInfo>("Could not find invitation")
                : Result.Ok(GetInvitationData(invitation).RegistrationInfo);
        }

        private static RegularCustomerInvitation GetInvitationData(CustomerInvitation customerInvitation)
        {
            return JsonConvert.DeserializeObject<RegularCustomerInvitation>(customerInvitation.Data);
        }
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITemplatedMailSender _mailSender;
        private readonly IRegistrationService _registrationService;
        private readonly ICustomerContext _customerContext;
        private readonly InvitationOptions _options;
    }
}