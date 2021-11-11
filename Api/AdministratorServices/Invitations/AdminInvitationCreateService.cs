using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Infrastructure;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices.Invitations
{
    public class AdminInvitationCreateService : IAdminInvitationCreateService
    {
        public AdminInvitationCreateService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            ILogger<AdminInvitationCreateService> logger,
            INotificationService notificationService,
            IOptions<AdminInvitationMailOptions> options,
            IInvitationRecordService invitationRecordService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _notificationService = notificationService;
            _options = options.Value;
            _invitationRecordService = invitationRecordService;
        }


        public Task<Result<string>> Create(UserInvitationData prefilledData, int inviterUserId)
        {
            if (prefilledData.RoleIds is null || !prefilledData.RoleIds.Any())
                return Task.FromResult(Result.Failure<string>("Invitation should have role ids"));
            
            var invitationCode = GenerateRandomCode();
            var now = _dateTimeProvider.UtcNow();
            var registrationInfo = prefilledData.UserRegistrationInfo;

            return SaveInvitation()
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
                    Email = registrationInfo.Email,
                    Created = now,
                    InviterUserId = inviterUserId,
                    InvitationType = UserInvitationTypes.Administrator,
                    InvitationStatus = UserInvitationStatuses.Active,
                    Data = JsonConvert.SerializeObject(prefilledData)
                };

                _context.UserInvitations.Add(newInvitation);

                await _context.SaveChangesAsync();

                return newInvitation;
            }


            void LogInvitationCreated()
                => _logger.LogInvitationCreated(UserInvitationTypes.Administrator, registrationInfo.Email);
        }


        public Task<Result<string>> Send(UserInvitationData prefilledData, int inviterUserId)
        {
            return Result.Success()
                .Ensure(AllProvidedRolesExist, "All admin roles should exist")
                .Bind(CreateInvitation)
                .Check(SendInvitationMail);


            async Task<bool> AllProvidedRolesExist()
            {
                var allRoles = await _context.AdministratorRoles.Select(x => x.Id).ToListAsync();
                return prefilledData.RoleIds.All(allRoles.Contains);
            }
            
            
            Task<Result<string>> CreateInvitation()
                => Create(prefilledData, inviterUserId);
            

            async Task<Result> SendInvitationMail(string invitationCode)
            {
                var registrationInfo = prefilledData.UserRegistrationInfo;
                var messagePayload = new InvitationData
                {
                    InvitationCode = invitationCode,
                    UserEmailAddress = registrationInfo.Email,
                    UserName = $"{registrationInfo.FirstName} {registrationInfo.LastName}",
                    FrontendBaseUrl = _options.FrontendBaseUrl
                };

                return await _notificationService.Send(messageData: messagePayload,
                    notificationType: NotificationTypes.AdministratorInvitation,
                    emails: new() { registrationInfo.Email });
            }
        }


        public Task<Result<string>> Resend(string oldInvitationCodeHash)
        {
            return _invitationRecordService.GetActiveInvitationByHash(oldInvitationCodeHash)
                .BindWithTransaction(_context, invitation => Result.Success(invitation)
                    .Check(SetOldInvitationResent)
                    .Bind(SendNewInvitation));


            Task<Result<string>> SendNewInvitation(UserInvitation oldInvitation)
                => Send(GetInvitationData(oldInvitation), oldInvitation.InviterUserId);


            Task<Result> SetOldInvitationResent(UserInvitation _)
                => _invitationRecordService.SetToResent(oldInvitationCodeHash);


            UserInvitationData GetInvitationData(UserInvitation invitation)
                => _invitationRecordService.GetInvitationData(invitation);
        }


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<AdminInvitationCreateService> _logger;
        private readonly INotificationService _notificationService;
        private readonly AdminInvitationMailOptions _options;
        private readonly IInvitationRecordService _invitationRecordService;
    }
}
