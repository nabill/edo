using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Models.Management
{
    public class AdministratorRegistrationService : IAdministratorRegistrationService
    {
        public AdministratorRegistrationService(IAdministratorInvitationService invitationService,
            EdoContext context)
        {
            _invitationService = invitationService;
            _context = context;
        }
        
        public async Task<Result> RegisterByInvitation(string invitationCode, string identity)
        {
            return await _invitationService.GetPendingInvitation(invitationCode)
                .OnSuccess(CreateAdministrator);

            Task CreateAdministrator(AdministratorInvitationInfo info)
            {
                _context.Administrators.Add(new Administrator
                {
                    Email = info.Email,
                    FirstName = info.FirstName,
                    LastName = info.LastName,
                    IdentityHash = HashGenerator.ComputeHash(identity),
                    Position = info.Position
                });
                return _context.SaveChangesAsync();
            }
        }
        
        private readonly IAdministratorInvitationService _invitationService;
        private readonly EdoContext _context;
    }
}