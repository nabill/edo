using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Management.Administrators;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorService : IAdministratorService
    {
        public AdministratorService(EdoContext context,
            IAdministratorContext administratorContext)
        {
            _context = context;
            _administratorContext = administratorContext;
        }


        public Task<Result<RichAdministratorInfo>> GetCurrentWithPermissions()
        {
            return _administratorContext.GetCurrent()
                .Map(AddPremissionsInfo);


            async Task<RichAdministratorInfo> AddPremissionsInfo(Administrator administrator)
                => new RichAdministratorInfo(
                    id: administrator.Id,
                    firstName: administrator.FirstName,
                    lastName: administrator.LastName,
                    position: administrator.Position,
                    administratorRoleIds: administrator.AdministratorRoleIds,
                    isActive: administrator.IsActive,
                    permissions: (await GetAvailablePermissions(administrator)).ToList()
                );
        }


        private async Task<AdministratorPermissions> GetAvailablePermissions(Administrator administrator)
        {
            var rolesPermissions = await _context.AdministratorRoles
                .Where(x => administrator.AdministratorRoleIds.Contains(x.Id))
                .Select(x => x.Permissions)
                .ToListAsync();

            return rolesPermissions.SelectMany(r => r.ToList()).Aggregate((a, b) => a | b);
        }


        private readonly EdoContext _context;
        private readonly IAdministratorContext _administratorContext;
    }
}