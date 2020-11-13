using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Management;
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
                return Result.Success();
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

            bool IsNotAccepted(InvitationBase invitation) => !invitation.IsAccepted;

            bool HasCorrectType(InvitationBase invitation) => invitation.InvitationType == invitationType;

            bool InvitationIsActual(InvitationBase invitation) => invitation.Created + _options.InvitationExpirationPeriod > _dateTimeProvider.UtcNow();
        }


        private static TInvitationData GetInvitationData<TInvitationData>(InvitationBase invitation)
        {
            var t = typeof(TInvitationData);

            switch (invitation.InvitationType)
            {
                case UserInvitationTypes.Agent:
                {
                    var inv = (UserInvitation) invitation;
                    var data = inv.Data.RegistrationInfo;
                    var agentEditableInfo = new AgentEditableInfo(data.Title, data.FirstName, data.LastName, data.Position, inv.Email);
                    return (TInvitationData) Convert.ChangeType(new AgentInvitationInfo(agentEditableInfo, inv.Data.AgencyId, inv.Email), t);
                }
                case UserInvitationTypes.Administrator:
                {
                    var inv = (AdminInvitation) invitation;
                    var data = inv.Data;
                    return (TInvitationData) Convert.ChangeType(new AdministratorInvitationInfo(inv.Email, data.LastName, data.FirstName, data.Position, data.Title), t);
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private async Task<Maybe<InvitationBase>> GetInvitation(string code)
        {
            var invitation = await _context.AllInvitations
                .SingleOrDefaultAsync(c => c.CodeHash == HashGenerator.ComputeSha256(code));

            return invitation ?? Maybe<InvitationBase>.None;
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
            switch (invitationInfo)
            {
                case AgentInvitationInfo info when invitationType == UserInvitationTypes.Agent:
                {
                    _context.UserInvitations.Add(new UserInvitation
                    {
                        CodeHash = HashGenerator.ComputeSha256(invitationCode),
                        Created = _dateTimeProvider.UtcNow(),
                        Data = new UserInvitation.UserInvitationData
                        {
                            AgencyId = info.AgencyId,
                            RegistrationInfo = new UserInvitation.UserRegistrationInfo
                            {
                                FirstName = info.RegistrationInfo.FirstName,
                                LastName = info.RegistrationInfo.LastName,
                                Title = info.RegistrationInfo.Title,
                                Position = info.RegistrationInfo.Position
                            }
                        },
                        Email = addresseeEmail,
                        InvitationType = invitationType
                    });
                }
                    break;

                case AdministratorInvitationInfo info when invitationType == UserInvitationTypes.Administrator:
                {
                    _context.AdminInvitations.Add(new AdminInvitation
                    {
                        CodeHash = HashGenerator.ComputeSha256(invitationCode),
                        Created = _dateTimeProvider.UtcNow(),
                        Data = new AdminInvitation.AdminInvitationData
                        {
                            Title = info.Title,
                            Email = info.Email,
                            FirstName = info.FirstName,
                            LastName = info.LastName
                        },
                        Email = addresseeEmail,
                        InvitationType = invitationType
                    });
                }
                    break;
            }

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