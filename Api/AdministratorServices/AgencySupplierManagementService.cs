using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
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
        var (_, isFailure, defaultSuppliers, error) = GetDefaultSuppliers();
        if (isFailure)
            return Result.Failure<Dictionary<string, bool>>(error);

        var agencySettings = await _context.AgencySupplierSettings
            .SingleOrDefaultAsync(a => a.AgencyId == agencyId);

        if (Equals(agencySettings, default))
            return defaultSuppliers;

        var agencySuppliers = agencySettings.EnabledSuppliers;
        var materializedSettings = new Dictionary<string, bool>();

        foreach (var (supplierCode, defaultOption) in defaultSuppliers)
        {
            bool materializedOption;
            
            if (!agencySuppliers.TryGetValue(supplierCode, out var agencyOption))
            {
                materializedOption = defaultOption;
            }
            else if (!defaultOption)
            {
                materializedOption = false;
            }
            else if (!agencyOption)
            {
                materializedOption = false;
            }
            else
            {
                materializedOption = true;
            }

            materializedSettings.Add(supplierCode, materializedOption);
        }

        return materializedSettings;
    }


    private Result<Dictionary<string, bool>> GetDefaultSuppliers()
    {
        var (_, isFailure, suppliers, error) = _supplierOptionsStorage.GetAll();
        return isFailure
            ? Result.Failure<Dictionary<string, bool>>(error)
            : suppliers.ToDictionary(s => s.Code, s => s.IsEnabled);
    }

    
    private readonly EdoContext _context;
    private readonly ISupplierOptionsStorage _supplierOptionsStorage;
}