using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.Management.Administrators;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.Administrators;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorManagementService : IAdministratorManagementService
    {
        public AdministratorManagementService(EdoContext context,
            IManagementAuditService managementAuditService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
        }


        public Task<List<AdministratorInfo>> GetAll()
            => _context.Administrators
                .Select(a => a.ToAdministratorInfo())
                .ToListAsync();


        public Task<Result> Activate(int administratorId, Administrator initiator)
            => SetActivityState(administratorId, initiator, true);


        public Task<Result> Deactivate(int administratorId, Administrator initiator)
            => SetActivityState(administratorId, initiator, false);


        public Task<List<AccountManager>> GetAccountManagers()
            => _context.Administrators
                .Where(a => a.AdministratorRoleIds.Contains(AccountManagerRole) && a.IsActive)
                .Select(a => a.ToAccountManager())
                .ToListAsync();


        private Task<Result> SetActivityState(int administratorId, Administrator initiator, bool isActive)
        {
            return Get(administratorId)
                .BindWithTransaction(_context, a => Result.Success(a)
                    .Tap(SetActivityState)
                    .Bind(WriteAuditLog));


            Task SetActivityState(Administrator administrator)
            {
                administrator.IsActive = isActive;
                _context.Administrators.Update(administrator);
                return _context.SaveChangesAsync();
            }


            Task<Result> WriteAuditLog(Administrator administrator)
                => _managementAuditService.Write(ManagementEventType.AdministratorChangeActivityState,
                    new AdministratorChangeActivityStateEventData { AdministratorId = initiator.Id, NewActivityState = isActive });
        }


        private async Task<Result<Administrator>> Get(int administratorId)
            => await _context.Administrators.SingleOrDefaultAsync(a => a.Id == administratorId)
                ?? Result.Failure<Administrator>("Administrator with specified Id does not exist");


        private const int AccountManagerRole = 1;

        private readonly EdoContext _context;
        private readonly IManagementAuditService _managementAuditService;
    }
}