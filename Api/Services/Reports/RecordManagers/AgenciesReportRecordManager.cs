using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers;

public class AgenciesReportRecordManager
{
    public AgenciesReportRecordManager(EdoContext context, IAgencySupplierManagementService agencySupplierManagementService)
    {
        _context = context;
        _agencySupplierManagementService = agencySupplierManagementService;
    }


    public async Task<IEnumerable<AgenciesReportRow>> Get()
    {
        var data = await (from agency in _context.Agencies
                join account in _context.AgencyAccounts on agency.Id equals account.AgencyId into accountGroup
                from account in accountGroup.DefaultIfEmpty()
                join markup in _context.MarkupPolicies on agency.Id.ToString() equals markup.SubjectScopeId into markupGroup
                from markup in markupGroup.DefaultIfEmpty()
                join settings in _context.AgencySystemSettings on agency.Id equals settings.AgencyId into settingsGroup
                from settings in settingsGroup.DefaultIfEmpty()
                where markup == null || (markup.SubjectScopeType == SubjectMarkupScopeTypes.Agency && 
                        (markup.DestinationScopeType == DestinationMarkupScopeTypes.Global 
                            || markup.DestinationScopeType == DestinationMarkupScopeTypes.NotSpecified))
                select new AgenciesReportData
                {
                    AgencyAccount = account,
                    Created = agency.Created,
                    AgencyId = agency.Id,
                    RootAgencyId = agency.Ancestors.Count > 0 ? agency.Ancestors.First() : null,
                    AgencyName = agency.Name,
                    AgencySystemSettings = settings,
                    City = agency.City,
                    ContractKind = agency.ContractKind,
                    CountryCode = agency.CountryCode,
                    GlobalMarkup = markup,
                    IsActive = agency.IsActive,
                    IsContractLoaded = agency.IsContractUploaded
                }).ToListAsync();

        return FillData(data);
    }


    private IEnumerable<AgenciesReportRow> FillData(IEnumerable<AgenciesReportData> rows)
    {
        var report = new List<AgenciesReportRow>();

        foreach (var row in rows)
        {
            var agencySuppliers = row.AgencySystemSettings?.EnabledSuppliers;
            var rootSuppliers = GetRootSuppliers(row);
            var materializedSuppliers = _agencySupplierManagementService.GetMaterializedSuppliers(agencySuppliers, rootSuppliers).Value;
            
            report.Add(new AgenciesReportRow
            {
                AgencyId = row.AgencyId,
                AgencyName = row.AgencyName,
                Created = row.Created.DateTime,
                Suppliers = StringifySuppliers(materializedSuppliers),
                RootAgencyId = row.RootAgencyId?.ToString() ?? "None",
                Balance = row.AgencyAccount?.Balance ?? 0,
                City = row.City,
                CountryCode = row.CountryCode,
                ContractKind = row.ContractKind?.ToString() ?? "Not specified",
                GlobalMarkup = (int) (row.GlobalMarkup?.Value ?? decimal.Zero),
                IsActive = row.IsActive ? "Yes" : "No",
                IsContractLoaded = row.IsContractLoaded ? "Yes" : "No"
            });
        }

        return SortByRootAgencies(report);


        Dictionary<string, bool>? GetRootSuppliers(AgenciesReportData row)
        {
            var rootAgencyId = row.RootAgencyId;
            if (rootAgencyId is null)
                return null;

            var rootRow = rows.FirstOrDefault(r => r.AgencyId == rootAgencyId);
            return rootRow.AgencySystemSettings?.EnabledSuppliers;
        }


        string StringifySuppliers(Dictionary<string, bool> suppliers)
        {
            var suppliersList = suppliers.Where(s => s.Value)
                .Select(s => s.Key)
                .ToList();
            
            return string.Join(", ", suppliersList);
        }

        
        // This will sort rows
        // root agency will go first and then all its children
        // then all other root agencies with children
        // and in the end - all agencies without children
        // Children will be sorted too
        List<AgenciesReportRow> SortByRootAgencies(List<AgenciesReportRow> rows)
        {
            var groups = rows.GroupBy(r => r.RootAgencyId);

            var rootsGroup = groups.First(g => g.Key == "None")
                .ToList()
                .OrderBy(r => r.AgencyId);
            
            var childrenGroups = groups.Where(g => g.Key != "None")
                .OrderBy(g => int.Parse(g.Key))
                .ToList();

            var sorted = new List<AgenciesReportRow>();
            var used = new List<int>();
            
            foreach (var group in childrenGroups)
            {
                var rootKey = int.Parse(group.Key);
                var rootElement = rootsGroup.First(r => r.AgencyId == rootKey);
                used.Add(rootKey);
                sorted.Add(rootElement);
                sorted.AddRange(group.OrderBy(g => g.AgencyId));
            }

            sorted.AddRange(rootsGroup.Where(row => !used.Contains(row.AgencyId)));

            return sorted;
        }
    }
    

    private readonly EdoContext _context;
    private readonly IAgencySupplierManagementService _agencySupplierManagementService;
}