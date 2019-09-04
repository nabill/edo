using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Emails;
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
            IMailSender mailSender,
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
        
        public async Task<Result> SendInvitation(RegularCustomerInvitation invitationInfo)
        {
            // TODO: move to authorization policies.
            if(!await _customerContext.IsMasterCustomer())
                return Result.Fail("Only master customers can send invitations");
            
            var invitationCode = GenerateRandomCode();
            var addresseeEmail = invitationInfo.RegistrationInfo.Email;
            
            return await SendInvitationMail()
                .OnSuccess(SaveInvitationData);
            
            string GenerateRandomCode()
            {
                using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
                {
                    var byteArray = new byte[64];
                    provider.GetBytes(byteArray);
                    return Convert.ToBase64String(byteArray)
                        .Replace("/", string.Empty);
                }
            }

            Task<Result> SendInvitationMail()
            {
                return _mailSender.Send(templateId: _options.MailTemplateId,
                    recipientAddress: addresseeEmail, 
                    messageData: new InvitationData { InvitationCode = invitationCode });
            }

            Task SaveInvitationData()
            {
                _context.CustomerInvitations.Add(new CustomerInvitation
                {
                    CodeHash = HashGenerator.ComputeHash(invitationCode),
                    Created = _dateTimeProvider.UtcNow(),
                    Data = JsonConvert.SerializeObject(invitationInfo),
                    Email = addresseeEmail
                });

                return _context.SaveChangesAsync();
            }
        }
        
        public async Task<Result> AcceptInvitation(CustomerRegistrationInfo customerRegistrationInfo, 
            string invitationCode, string identity)
        {
            return await GetInvitationByCode(invitationCode)
                .ToResult("Could not find invitation")
                .Ensure(invitation => !invitation.IsAccepted, "Invitation is already accepted")
                .Ensure(InvitationIsActual, "Invitation expired")
                .OnSuccessWithTransactionScope(invitation => Result.Ok(invitation)
                    .OnSuccess(RegisterRegularCustomer)
                    .OnSuccess(SetInvitationAccepted));

            Task<Result<CustomerInvitation>> RegisterRegularCustomer(CustomerInvitation invitation)
            {
                var invitationData = GetInvitationData(invitation);
                return _registrationService
                    .RegisterRegularCustomer(customerRegistrationInfo, invitationData.CompanyId, identity)
                    .OnSuccess(() => invitation);
            }

            Task<int> SetInvitationAccepted(CustomerInvitation invitation)
            {
                invitation.IsAccepted = true;
                _context.Update(invitation);
                return _context.SaveChangesAsync();
            }

            bool InvitationIsActual(CustomerInvitation invitation)
            {
                return invitation.Created + _options.InvitationExpirationPeriod > _dateTimeProvider.UtcNow();
            }
        }

        public async Task<Result<CustomerRegistrationInfo>> GetInvitationInfo(string invitationCode)
        {
            var invitation = await GetInvitationByCode(invitationCode);

            return invitation.HasValue
                ? Result.Ok(GetInvitationData(invitation.Value).RegistrationInfo)
                : Result.Fail<CustomerRegistrationInfo>("Could not find invitation");
        }
        
        private async Task<Maybe<CustomerInvitation>> GetInvitationByCode(string invitationCode)
        {
            var invitation = await _context.CustomerInvitations
                .SingleOrDefaultAsync(c => c.CodeHash == HashGenerator.ComputeHash(invitationCode));
            
            return invitation ?? Maybe<CustomerInvitation>.None;
        }

        private static RegularCustomerInvitation GetInvitationData(CustomerInvitation customerInvitation)
        {
            return JsonConvert.DeserializeObject<RegularCustomerInvitation>(customerInvitation.Data);
        }
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMailSender _mailSender;
        private readonly IRegistrationService _registrationService;
        private readonly ICustomerContext _customerContext;
        private readonly InvitationOptions _options;
    }
}