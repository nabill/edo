using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.SupplierOptionsClient.Models;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.EntityFrameworkCore;

namespace Api.AdministratorServices
{
    public class AgentSupplierManagementService : IAgentSupplierManagementService
    {
        public AgentSupplierManagementService(EdoContext context,
            IAgencySupplierManagementService agencySupplierManagementService,
            ISupplierOptionsStorage supplierOptionsStorage)
        {
            _context = context;
            _agencySupplierManagementService = agencySupplierManagementService;
            _supplierOptionsStorage = supplierOptionsStorage;
        }


        public async Task<Result<Dictionary<string, bool>>> GetMaterializedSuppliers(int agencyId, int agentId)
        {
            var (_, isFailure, enabledSuppliers, error) = GetEnabledSuppliers();
            if (isFailure)
                return Result.Failure<Dictionary<string, bool>>(error);

            var agentSettings = await _context.AgentSystemSettings
                .SingleOrDefaultAsync(a => a.AgentId == agentId && a.AgencyId == agencyId);

            var (_, isNotSuccess, agencySuppliers, errorMessage) = await _agencySupplierManagementService
                .GetMaterializedSuppliers(agencyId);
            if (isFailure)
                return Result.Failure<Dictionary<string, bool>>(errorMessage);
            agencySuppliers = agencySuppliers
                .Where(s => s.Value)
                .ToDictionary(s => s.Key, s => s.Value);

            if (Equals(agentSettings?.EnabledSuppliers, null))
            {
                var suppliersFromSunpu = enabledSuppliers
                    .Where(s => s.Value == EnablementState.Enabled)
                    .ToDictionary(s => s.Key, s => true);

                return _agencySupplierManagementService.GetIntersection(agencySuppliers, suppliersFromSunpu);
            }

            var agentSuppliers = _agencySupplierManagementService.SunpuMaterialization(agentSettings.EnabledSuppliers, enabledSuppliers);
            var resultSuppliers = _agencySupplierManagementService.GetIntersection(agencySuppliers, agentSuppliers);

            return resultSuppliers;
        }


        public async Task<Result> SaveSuppliers(int agencyId, int agentId, Dictionary<string, bool> suppliers)
        {
            return await Validate()
                .Tap(AddOrUpdate);


            async Task<Result> Validate()
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == agentId);
                if (Equals(agent, default))
                    return Result.Failure($"Agent {agentId} does not exist");

                var enabledSuppliers = GetEnabledSuppliers().Value;

                foreach (var (name, _) in suppliers)
                {
                    if (!enabledSuppliers.ContainsKey(name))
                        return Result.Failure($"Supplier {name} does not exist or is disabled in the system");
                }

                return Result.Success();
            }


            async Task AddOrUpdate()
            {
                var agentSystemSettings = await _context.AgentSystemSettings
                    .SingleOrDefaultAsync(a => a.AgentId == agentId && a.AgencyId == agencyId);
                if (Equals(agentSystemSettings, default))
                {
                    var settings = new AgentSystemSettings
                    {
                        AgentId = agentId,
                        AgencyId = agencyId,
                        EnabledSuppliers = suppliers
                    };
                    _context.Add(settings);
                }
                else
                {
                    agentSystemSettings.EnabledSuppliers = suppliers;
                    _context.Attach(agentSystemSettings)
                        .Property(s => s.EnabledSuppliers)
                        .IsModified = true;
                }

                await _context.SaveChangesAsync();
            }
        }


        private Result<Dictionary<string, EnablementState>> GetEnabledSuppliers()
        {
            var (_, isFailure, suppliers, error) = _supplierOptionsStorage.GetAll();
            return isFailure
                ? Result.Failure<Dictionary<string, EnablementState>>(error)
                : suppliers.Where(s => s.EnablementState is EnablementState.Enabled or EnablementState.TestOnly)
                    .ToDictionary(s => s.Code, s => s.EnablementState);
        }


        private readonly EdoContext _context;
        private readonly IAgencySupplierManagementService _agencySupplierManagementService;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}