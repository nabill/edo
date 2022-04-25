using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices;

public class AgencySupplierManagementService : IAgencySupplierManagementService
{
    public AgencySupplierManagementService(EdoContext context, ISupplierOptionsStorage supplierOptionsStorage)
    {
        _context = context;
        _supplierOptionsStorage = supplierOptionsStorage;
    }
    

    public async Task<Result<Dictionary<string, bool>>> GetMaterializedSuppliers(int agencyId)
    {
        var (_, isFailure, enabledSuppliers, error) = GetEnabledSupplierCodes();
        if (isFailure)
            return Result.Failure<Dictionary<string, bool>>(error);

        var agencySettings = await _context.AgencySystemSettings
            .SingleOrDefaultAsync(a => a.AgencyId == agencyId);

        if (Equals(agencySettings, default) || Equals(agencySettings.EnabledSuppliers, null))
            return enabledSuppliers.ToDictionary(s => s, _ => true);

        var agencySuppliers = agencySettings.EnabledSuppliers;
        var materializedSettings = new Dictionary<string, bool>();

        foreach (var supplierCode in enabledSuppliers)
        {
            var materializedOption = !agencySuppliers.TryGetValue(supplierCode, out var agencyOption) || agencyOption;
            materializedSettings.Add(supplierCode, materializedOption);
        }

        return materializedSettings;
    }


    public async Task<Result> SaveSuppliers(int agencyId, Dictionary<string, bool> suppliers)
    {
        return await Validate()
            .Tap(AddOrUpdate);
        
        
        async Task<Result> Validate()
        {
            var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);
            if (Equals(agency, default))
                return Result.Failure($"Agency {agencyId} does not exist");
            
            var enabledSuppliers = GetEnabledSupplierCodes().Value;
            
            foreach (var (name, _) in suppliers)
            {
                if (!enabledSuppliers.Contains(name))
                    return Result.Failure($"Supplier {name} does not exist or is disabled in the system");
            }
            
            return Result.Success();
        }


        async Task AddOrUpdate()
        {
            var agencySystemSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(a => a.AgencyId == agencyId);
            if (Equals(agencySystemSettings, default))
            {
                var settings = new AgencySystemSettings
                {
                    AgencyId = agencyId,
                    EnabledSuppliers = suppliers
                };
                _context.Add(settings);
            }
            else
            {
                agencySystemSettings.EnabledSuppliers = suppliers;
                _context.Attach(agencySystemSettings)
                    .Property(s => s.EnabledSuppliers)
                    .IsModified = true;
            }

            await _context.SaveChangesAsync();
        }
    }


    private Result<List<string>> GetEnabledSupplierCodes()
    {
        var (_, isFailure, suppliers, error) = _supplierOptionsStorage.GetAll();
        return isFailure
            ? Result.Failure<List<string>>(error)
            : suppliers.Where(s => s.IsEnabled).Select(s => s.Code).ToList();
    }

    
    private readonly EdoContext _context;
    private readonly ISupplierOptionsStorage _supplierOptionsStorage;
}