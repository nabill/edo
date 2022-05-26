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
        var (_, isFailure, enabledSuppliers, error) = GetEnabledSuppliers();
        if (isFailure)
            return Result.Failure<Dictionary<string, bool>>(error);

        var agencySettings = await _context.AgencySystemSettings
            .SingleOrDefaultAsync(a => a.AgencyId == agencyId);

        var rootAgencySuppliers = await GetRootSuppliers(agencyId, enabledSuppliers);

        if (Equals(agencySettings?.EnabledSuppliers, null))
            return rootAgencySuppliers;

        var agencySuppliers = SunpuMaterialization(agencySettings.EnabledSuppliers, enabledSuppliers);
        var resultSuppliers = GetIntersection(rootAgencySuppliers!, agencySuppliers);

        return resultSuppliers;
    }


    public async Task<Dictionary<string, bool>> GetRootSuppliers(int agencyId,
        Dictionary<string, EnablementState> enabledSuppliers)
    {
        var rootAgencyId = await _context.Agencies
            .Where(a => a.Id == agencyId)
            .Select(a => a.ParentId)
            .SingleOrDefaultAsync();

        if (rootAgencyId == default)
            return enabledSuppliers
                .Where(s => s.Value == EnablementState.Enabled)
                .ToDictionary(s => s.Key, s => true);

        var rootAgencySettings = await _context.AgencySystemSettings
            .SingleOrDefaultAsync(a => a.AgencyId == rootAgencyId);

        if (Equals(rootAgencySettings?.EnabledSuppliers, null))
            return enabledSuppliers
                .Where(s => s.Value == EnablementState.Enabled)
                .ToDictionary(s => s.Key, s => true);

        var rootAgencySuppliers = rootAgencySettings.EnabledSuppliers;

        var resultSuppliers = SunpuMaterialization(rootAgencySuppliers, enabledSuppliers);

        return resultSuppliers;
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

            var resultValue = (!rootValue) ? rootValue : childValue;
            result.Add(rootKey, resultValue);
        }

        var childLeftSuppliers = childAgencySuppliers
            .Where(s => !result.ContainsKey(s.Key))
            .ToDictionary(s => s.Key, s => s.Value);

        if (childLeftSuppliers != default)
            result = result.Union(childLeftSuppliers).ToDictionary(s => s.Key, s => s.Value);

        return result;
    }


    public Dictionary<string, bool> SunpuMaterialization(Dictionary<string, bool> suppliers,
        Dictionary<string, EnablementState> enabledSuppliers)
    {
        var materializedSettings = new Dictionary<string, bool>();

        foreach (var (supplier, supplierOption) in enabledSuppliers)
        {
            var settingExist = suppliers.TryGetValue(supplier, out var agencyOption);
            var materializedOption = supplierOption switch
            {
                EnablementState.TestOnly => agencyOption,
                EnablementState.Enabled => agencyOption || !settingExist,
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