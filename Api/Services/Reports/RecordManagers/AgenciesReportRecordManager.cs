using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers;

public class AgenciesReportRecordManager
{
    public AgenciesReportRecordManager(EdoContext context)
    {
        _context = context;
    }


    public async Task<IEnumerable<AgenciesReportData>> Get()
    {
        return await (from agency in _context.Agencies
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
    }


    private readonly EdoContext _context;
}