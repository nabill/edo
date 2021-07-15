using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Infrastructure;
using HappyTravel.Edo.Data.Management;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices.Invitations
{
    public class AdminInvitationAcceptService : IAdminInvitationAcceptService
    {
        public AdminInvitationAcceptService(
            IInvitationRecordService invitationRecordService,
            EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IManagementAuditService managementAuditService)
        {
            _invitationRecordService = invitationRecordService;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _managementAuditService = managementAuditService;
        }


        public async Task<Result> Accept(string invitationCode, string identity, string email)
        {
            return await GetActiveInvitation()
                .Ensure(IsIdentityPresent, "User should have identity")
                .Ensure(IsInvitationTypeCorrect, "Incorrect invitation type")
                .Ensure(IsEmailUnique, "Administrator with same mail is already registered")
                .BindWithTransaction(_context, invitation => Result.Success(invitation)
                    .Tap(SaveAccepted)
                    .Bind(CreateAdmin)
                    .Tap(WriteAuditLog));


            Task<Result<UserInvitation>> GetActiveInvitation() 
                => _invitationRecordService.GetActiveInvitationByCode(invitationCode);


            bool IsIdentityPresent(UserInvitation _) 
                => !string.IsNullOrWhiteSpace(identity);


            bool IsInvitationTypeCorrect(UserInvitation invitation) 
                => invitation.InvitationType == UserInvitationTypes.Administrator;


            async Task<bool> IsEmailUnique(UserInvitation _) 
                => !await _context.Administrators.AnyAsync(a => a.Email == email);


            Task SaveAccepted(UserInvitation _) 
                => _invitationRecordService.SetAccepted(invitationCode);


            async Task<Result<Administrator>> CreateAdmin(UserInvitation invitation)
            {
                var now = _dateTimeProvider.UtcNow();
                var invitationData = _invitationRecordService.GetInvitationData(invitation);

                var administrator = new Administrator
                {
                    Email = email,
                    FirstName = invitationData.UserRegistrationInfo.FirstName,
                    LastName = invitationData.UserRegistrationInfo.LastName,
                    IdentityHash = HashGenerator.ComputeSha256(identity),
                    Position = invitationData.UserRegistrationInfo.Position,
                    AdministratorRoleIds = invitationData.RoleIds,
                    Created = now,
                    Updated = now
                };

                _context.Administrators.Add(administrator);
                await _context.SaveChangesAsync();

                return administrator;
            }


            Task WriteAuditLog(Administrator administrator)
                => _managementAuditService.Write(ManagementEventType.AdministratorRegistration,
                    new AdministrationRegistrationEvent(administrator.Email, administrator.Id, invitationCode));
        }


        private readonly IInvitationRecordService _invitationRecordService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IManagementAuditService _managementAuditService;
    }
}
