using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Infrastructure;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public class AgentInvitationCreateService : IAgentInvitationCreateService
    {
        public AgentInvitationCreateService(EdoContext context, IDateTimeProvider dateTimeProvider,
            ILogger<AgentInvitationCreateService> logger, INotificationService notificationService,
            IInvitationRecordService invitationRecordService, IAdminAgencyManagementService agencyManagementService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _notificationService = notificationService;
            _invitationRecordService = invitationRecordService;
            _agencyManagementService = agencyManagementService;
        }


        public Task<Result<string>> Create(UserInvitationData prefilledData, UserInvitationTypes invitationType,
            int inviterUserId, int? inviterAgencyId = null)
        {
            var invitationCode = GenerateRandomCode();
            var now = _dateTimeProvider.UtcNow();

            return Result.Success()
                .Ensure(AllProvidedRolesExist, "All roles should exist")
                .Bind(SaveInvitation)
                .Tap(LogInvitationCreated)
                .Map(_ => invitationCode);


            string GenerateRandomCode()
            {
                using var provider = new RNGCryptoServiceProvider();

                var byteArray = new byte[64];
                provider.GetBytes(byteArray);

                return Base64UrlEncoder.Encode(byteArray);
            }


            async Task<bool> AllProvidedRolesExist()
            {
                var allRoleIds = await _context.AgentRoles.Select(r => r.Id).ToListAsync();

                return prefilledData.RoleIds.All(x => allRoleIds.Contains(x));
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


            void LogInvitationCreated() 
                => _logger.LogInvitationCreated(invitationType, prefilledData.UserRegistrationInfo.Email);
        }


        public Task<Result<string>> Send(UserInvitationData prefilledData, UserInvitationTypes invitationType,
            int inviterUserId, int? inviterAgencyId = null)
        {
            return Validate()
                .Bind(CreateInvitation)
                .Check(SendInvitationMailPipe);

            Task<Result> SendInvitationMailPipe(string invitationCode) 
                => SendInvitationMail(invitationCode, prefilledData, invitationType, inviterAgencyId);


            Result Validate()
                => GenericValidator<UserInvitationData>.Validate(v =>
                {
                    v.RuleFor(x => x.UserRegistrationInfo.FirstName).NotEmpty()
                        .WithMessage("FirstName is required");
                    v.RuleFor(x => x.UserRegistrationInfo.LastName).NotEmpty()
                        .WithMessage("LastName is required");
                    v.RuleFor(x => x.UserRegistrationInfo.Title).NotEmpty()
                        .WithMessage("Title is required");
                    v.RuleFor(e => e.UserRegistrationInfo.Email).NotEmpty().EmailAddress()
                        .WithMessage("Valid email is required");
                }, prefilledData);


            Task<Result<string>> CreateInvitation() 
                => Create(prefilledData, invitationType, inviterUserId, inviterAgencyId);
        }


        public Task<Result<string>> Recreate(string oldInvitationCodeHash)
        {
            return _invitationRecordService.GetActiveInvitationByHash(oldInvitationCodeHash)
                .BindWithTransaction(_context, invitation => Result.Success(invitation)
                    .Check(SetOldInvitationResent)
                    .Bind(CreateNewInvitation));

            Task<Result> SetOldInvitationResent(UserInvitation _) 
                => _invitationRecordService.SetToResent(oldInvitationCodeHash);


            Task<Result<string>> CreateNewInvitation(UserInvitation oldInvitation)
                => Create(GetInvitationData(oldInvitation), oldInvitation.InvitationType, oldInvitation.InviterUserId, oldInvitation.InviterAgencyId);
        }


        public Task<Result<string>> Resend(string oldInvitationCodeHash)
        {
            return Recreate(oldInvitationCodeHash)
                .Check(SendInvitationMailPipe);


            Task<Result> SendInvitationMailPipe(string newInvitationCode)
                => _invitationRecordService.GetActiveInvitationByCode(newInvitationCode)
                    .Bind(i => SendInvitationMail(newInvitationCode, GetInvitationData(i), i.InvitationType, i.InviterAgencyId));
        }


        private UserInvitationData GetInvitationData(UserInvitation invitation) 
            => _invitationRecordService.GetInvitationData(invitation);


        private async Task<Result> SendInvitationMail(string invitationCode, UserInvitationData prefilledData,
            UserInvitationTypes invitationType, int? inviterAgencyId)
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

            var notificationType = invitationType switch
            {
                UserInvitationTypes.Agent => NotificationTypes.AgentInvitation,
                UserInvitationTypes.ChildAgency => NotificationTypes.ChildAgencyInvitation,
                _ => NotificationTypes.None
            };

            return await _notificationService.Send(messageData: messagePayload,
                notificationType: notificationType,
                email: prefilledData.UserRegistrationInfo.Email);
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<AgentInvitationCreateService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IInvitationRecordService _invitationRecordService;
        private readonly IAdminAgencyManagementService _agencyManagementService;
    }
}