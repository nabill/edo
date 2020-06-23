using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.MailSender;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Users
{
    public class UserInvitationService : IUserInvitationService
    {
        public UserInvitationService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            MailSenderWithCompanyInfo mailSender,
            ILogger<UserInvitationService> logger,
            IOptions<UserInvitationOptions> options)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _mailSender = mailSender;
            _logger = logger;
            _options = options.Value;
        }


        public async Task<Result> Send<TInvitationData>(string email,
            TInvitationData invitationInfo,
            Func<TInvitationData, string, DataWithCompanyInfo> messagePayloadGenerator, 
            string mailTemplateId,
            UserInvitationTypes invitationType)
        {
            var invitationCode = GenerateRandomCode();

            return await SendInvitationMail()
                .Tap(SaveInvitation)
                .Tap(LogInvitationCreated);

            Task<Result> SendInvitationMail()
            {
                var messagePayload = messagePayloadGenerator(invitationInfo, invitationCode);
                
                return _mailSender.Send(mailTemplateId,
                    email,
                    messagePayload);
            }

            Task SaveInvitation() => SaveInvitationData(email, invitationInfo, invitationType, invitationCode);
            
            void LogInvitationCreated() => this.LogInvitationCreated(email);
        }


        public Task<Result<string>> Create<TInvitationData>(string email, TInvitationData invitationInfo, UserInvitationTypes invitationType)
        {
            var invitationCode = GenerateRandomCode();

            return SaveInvitation()
                .Tap(LogInvitationCreated)
                .Map(ReturnCode);
            
            async Task<Result> SaveInvitation()
            {
                await SaveInvitationData(email, invitationInfo, invitationType, invitationCode);
                return Result.Ok();
            }

            void LogInvitationCreated() => this.LogInvitationCreated(email);
            
            string ReturnCode() => invitationCode;
        }


        public async Task Accept(string invitationCode)
        {
            var invitationMaybe = await GetInvitation(invitationCode);
            if (invitationMaybe.HasValue)
            {
                var invitation = invitationMaybe.Value;
                invitation.IsAccepted = true;
                _context.Update(invitation);
                await _context.SaveChangesAsync();
            }
        }


        public Task<Result<TInvitationData>> GetPendingInvitation<TInvitationData>(string invitationCode, UserInvitationTypes invitationType)
        {
            return GetInvitation(invitationCode).ToResult("Could not find invitation")
                .Ensure(IsNotAccepted, "Already accepted")
                .Ensure(HasCorrectType, "Invitation type mismatch")
                .Ensure(InvitationIsActual, "Invitation expired")
                .Map(GetInvitationData<TInvitationData>);

            bool IsNotAccepted(UserInvitation invitation) => !invitation.IsAccepted;

            bool HasCorrectType(UserInvitation invitation) => invitation.InvitationType == invitationType;

            bool InvitationIsActual(UserInvitation invitation) => invitation.Created + _options.InvitationExpirationPeriod > _dateTimeProvider.UtcNow();
        }


        private static TInvitationData GetInvitationData<TInvitationData>(UserInvitation invitation)
            => JsonConvert.DeserializeObject<TInvitationData>(invitation.Data);


        private async Task<Maybe<UserInvitation>> GetInvitation(string code)
        {
            var invitation = await _context.UserInvitations
                .SingleOrDefaultAsync(c => c.CodeHash == HashGenerator.ComputeSha256(code));

            return invitation ?? Maybe<UserInvitation>.None;
        }
        
        
        private string GenerateRandomCode()
        {
            using var provider = new RNGCryptoServiceProvider();
                
            var byteArray = new byte[64];
            provider.GetBytes(byteArray);

            return Base64UrlEncoder.Encode(byteArray);
        }
        
        
        private Task SaveInvitationData<TInvitationData>(string addresseeEmail, TInvitationData invitationInfo, 
            UserInvitationTypes invitationType, string invitationCode)
        {
            _context.UserInvitations.Add(new UserInvitation
            {
                CodeHash = HashGenerator.ComputeSha256(invitationCode),
                Created = _dateTimeProvider.UtcNow(),
                Data = JsonConvert.SerializeObject(invitationInfo),
                Email = addresseeEmail,
                InvitationType = invitationType
            });

            return _context.SaveChangesAsync();
        }
        
        
        private void LogInvitationCreated(string email)
            => _logger.LogInvitationCreated(
                $"The invitation created for the user '{email}'");


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<UserInvitationService> _logger;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly UserInvitationOptions _options;
    }
}