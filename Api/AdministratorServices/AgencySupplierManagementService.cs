using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.SupplierOptionsClient.Models;
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
        var agencySuppliers = await GetAgencySuppliers();
        var rootSuppliers = await GetRootSuppliers();
        
        return await GetMaterializedSuppliers(agencySuppliers, rootSuppliers);


        async Task<Dictionary<string, bool>?> GetAgencySuppliers()
        {
            var agencySystemSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(a => a.AgencyId == agencyId);
            return agencySystemSettings?.EnabledSuppliers;
        }


        async Task<Dictionary<string, bool>?> GetRootSuppliers()
        {
            var agency = await _context.Agencies.SingleAsync(a => a.Id == agencyId);

            var rootAgencyId = agency.ParentId;
            if (rootAgencyId == default)
                return null;

            var rootAgencySystemSettings = await _context.AgencySystemSettings
                .SingleOrDefaultAsync(a => a.AgencyId == rootAgencyId);

            return rootAgencySystemSettings?.EnabledSuppliers;
        }
    }


    public async Task<Result<Dictionary<string, bool>>> GetMaterializedSuppliers(Dictionary<string, bool>? agencySuppliers,
        Dictionary<string, bool>? rootSuppliers)
    {
        var (_, isFailure, enabledSuppliers, error) = GetEnabledSuppliers();
        if (isFailure)
            return Result.Failure<Dictionary<string, bool>>(error);

        var materializedRootSuppliers = MaterializeRootSuppliers();
        return MaterializeAgencySuppliers();

        
        Dictionary<string, bool> MaterializeRootSuppliers()
        {
            if (rootSuppliers is null)
                return enabledSuppliers
                    .ToDictionary(s => s.Key, s => true);

            return SunpuMaterialization(rootSuppliers, enabledSuppliers, true)
                .Where(s => s.Value)
                .ToDictionary(s => s.Key, s => s.Value);
        }
        
        Dictionary<string, bool> MaterializeAgencySuppliers()
        {
            if (Equals(agencySuppliers, null))
                return GetIntersection(materializedRootSuppliers, SunpuMaterialization(materializedRootSuppliers, enabledSuppliers, false));
            
            return GetIntersection(materializedRootSuppliers, SunpuMaterialization(agencySuppliers, enabledSuppliers, true));
        }
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


    public Dictionary<string, bool> GetIntersection(Dictionary<string, bool> rootAgencySuppliers,
        Dictionary<string, bool> childAgencySuppliers)
    {
        var result = new Dictionary<string, bool>();

        foreach (var (rootKey, rootValue) in rootAgencySuppliers)
        {
            var (childKey, childValue) = childAgencySuppliers.FirstOrDefault(s => s.Key == rootKey);
            if (childKey == null)
            {
                result.Add(rootKey, rootValue);
                continue;
            }

            result.Add(rootKey, rootValue && childValue);
        }

        return result;
    }


    public Dictionary<string, bool> SunpuMaterialization(Dictionary<string, bool> suppliers,
        Dictionary<string, EnablementState> enabledSuppliers, bool withTestOnly)
    {
        var materializedSettings = new Dictionary<string, bool>();

        foreach (var (supplier, supplierOption) in enabledSuppliers)
        {
            var settingExist = suppliers.TryGetValue(supplier, out var agencyOption);
            var materializedOption = supplierOption switch
            {
                EnablementState.TestOnly => withTestOnly && agencyOption,
                EnablementState.Enabled => agencyOption || !settingExist,
                EnablementState.Disabled => false,
                _ => throw new ArgumentOutOfRangeException($"Incorrect supplierOption {supplierOption}")
            };

            materializedSettings.Add(supplier, materializedOption);
        }

        return materializedSettings;
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
    private readonly ISupplierOptionsStorage _supplierOptionsStorage;
}