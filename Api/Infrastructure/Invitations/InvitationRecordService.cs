using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Infrastructure.Invitations
{
    public class InvitationRecordService : IInvitationRecordService
    {
        public InvitationRecordService(
            IDateTimeProvider dateTimeProvider,
            EdoContext context,
            ILogger<InvitationRecordService> logger,
            IOptions<InvitationRecordOptions> options,
            MailSenderWithCompanyInfo mailSender,
            IAgencyManagementService agencyManagementService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _mailSender = mailSender;
            _options = options.Value;
            _logger = logger;
            _agencyManagementService = agencyManagementService;
        }


        public Task<Result<string>> Create(UserInvitationData prefilledData, UserInvitationTypes invitationType,
            bool shouldSendInvitationMail, int inviterUserId, int? inviterAgencyId = null)
        {
            var invitationCode = GenerateRandomCode();
            var now = _dateTimeProvider.UtcNow();

            return SaveInvitation()
                .CheckIf(shouldSendInvitationMail, SendInvitationMail)
                .Tap(LogInvitationCreated)
                .Map(_ => invitationCode);


            string GenerateRandomCode()
            {
                using var provider = new RNGCryptoServiceProvider();

                var byteArray = new byte[64];
                provider.GetBytes(byteArray);

                return Base64UrlEncoder.Encode(byteArray);
            }


            async Task<Result<UserInvitation>> SaveInvitation()
            {
                var newInvitation = new UserInvitation
                {
                    CodeHash = HashGenerator.ComputeSha256(invitationCode),
                    Email = prefilledData.UserRegistrationInfo.Email,
                    Created = now,
                    InviterUserId = inviterUserId,
                    InviterAgencyId = inviterAgencyId,
                    InvitationType = invitationType,
                    InvitationStatus = UserInvitationStatuses.Active,
                    Data = JsonConvert.SerializeObject(prefilledData)
                };

                _context.UserInvitations.Add(newInvitation);

                await _context.SaveChangesAsync();

                return newInvitation;
            }


            async Task<Result> SendInvitationMail(UserInvitation newInvitation)
            {
                string agencyName = null;
                if (inviterAgencyId.HasValue)
                {
                    var getAgencyResult = await _agencyManagementService.Get(inviterAgencyId.Value);
                    if (getAgencyResult.IsFailure)
                        return Result.Failure("Could not find inviter agency");

                    agencyName = getAgencyResult.Value.Name;
                }

                var messagePayload = new InvitationData
                {
                    AgencyName = agencyName,
                    InvitationCode = invitationCode,
                    UserEmailAddress = prefilledData.UserRegistrationInfo.Email,
                    UserName = $"{prefilledData.UserRegistrationInfo.FirstName} {prefilledData.UserRegistrationInfo.LastName}"
                };

                var templateId = GetTemplateId();
                if (string.IsNullOrWhiteSpace(templateId))
                    return Result.Failure("Could not find invitation mail template");

                return await _mailSender.Send(templateId,
                    prefilledData.UserRegistrationInfo.Email,
                    messagePayload);
            }


            string GetTemplateId()
                => invitationType switch
                {
                    UserInvitationTypes.Agent => _options.AgentInvitationTemplateId,
                    UserInvitationTypes.Administrator => _options.AdminInvitationTemplateId,
                    UserInvitationTypes.ChildAgency => _options.ChildAgencyInvitationTemplateId,
                    _ => null
                };


            void LogInvitationCreated()
                => _logger.LogInvitationCreated(
                    $"The invitation with type {invitationType} created for the user '{prefilledData.UserRegistrationInfo.Email}'");
        }


        public async Task<Result> Revoke(string code)
        {
            return await GetActiveInvitation(code)
                .Tap(SaveRevoked);


            Task SaveRevoked(UserInvitation invitation)
            {
                invitation.InvitationStatus = UserInvitationStatuses.Revoked;
                _context.Update(invitation);
                return _context.SaveChangesAsync();
            }
        }


        public Task<Result<string>> Resend(string code)
        {
            return GetActiveInvitation(code)
                .Bind(SendNewInvitation)
                .Tap(r => SaveResent(r.oldInvitation))
                .Map(r => r.newCode);


            async Task<Result<(UserInvitation oldInvitation, string newCode)>> SendNewInvitation(UserInvitation oldInvitation)
            {
                var (_, isFailure, newCode, error) = await Create(GetInvitationData(oldInvitation),
                    oldInvitation.InvitationType,
                    shouldSendInvitationMail: true,
                    oldInvitation.InviterUserId,
                    oldInvitation.InviterAgencyId);

                if (isFailure)
                    return Result.Failure<(UserInvitation, string)>(error);

                return (oldInvitation, newCode);
            }


            Task SaveResent(UserInvitation oldInvitation)
            {
                oldInvitation.InvitationStatus = UserInvitationStatuses.Resent;
                _context.Update(oldInvitation);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> SetAccepted(string code)
        {
            return await GetActiveInvitation(code)
                .Tap(SaveAccepted);


            Task SaveAccepted(UserInvitation invitation)
            {
                invitation.InvitationStatus = UserInvitationStatuses.Accepted;
                _context.Update(invitation);
                return _context.SaveChangesAsync();
            }
        }


        public Task<Result<UserInvitation>> GetActiveInvitation(string code)
        {
            return GetInvitation()
                .Ensure(InvitationIsActual, "Invitation expired");


            async Task<Result<UserInvitation>> GetInvitation()
            {
                var invitation = await _context.UserInvitations
                    .SingleOrDefaultAsync(i
                        => i.CodeHash == HashGenerator.ComputeSha256(code)
                        && i.InvitationStatus == UserInvitationStatuses.Active);

                return invitation ?? Result.Failure<UserInvitation>("Invitation with specified code either does not exist, or is not active.");
            }


            bool InvitationIsActual(UserInvitation invitation)
                => invitation.Created + _invitationExpirationPeriod > _dateTimeProvider.UtcNow();
        }


        public UserInvitationData GetInvitationData(UserInvitation invitation)
            => JsonConvert.DeserializeObject<UserInvitationData>(invitation.Data);


        private readonly TimeSpan _invitationExpirationPeriod = TimeSpan.FromDays(7);

        private readonly InvitationRecordOptions _options;
        private readonly ILogger<InvitationRecordService> _logger;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAgencyManagementService _agencyManagementService;
    }
}
