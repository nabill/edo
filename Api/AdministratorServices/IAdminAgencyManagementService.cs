using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAdminAgencyManagementService
    {
        Task<Result> DeactivateAgency(int agencyId, string reason);

        Task<Result> ActivateAgency(int agencyId, string reason);

        Task<Result<AgencyInfo>> Get(int agencyId, string languageCode = LocalizationHelper.DefaultLanguageCode);

        Task<Result<AgencyInfo>> GetRoot(int agencyId, string languageCode = LocalizationHelper.DefaultLanguageCode);

        IQueryable<AdminViewAgencyInfo> GetRootAgencies(string languageCode = LocalizationHelper.DefaultLanguageCode);

        Task<List<AgencyInfo>> GetChildAgencies(int parentAgencyId, string languageCode = LocalizationHelper.DefaultLanguageCode);

        Task<Result<ContractKind>> GetContractKind(int agencyId);

        Task<Result<AgencyVerificationStates>> GetVerificationState(int agencyId);

        Task<Result<AgencyInfo>> Edit(int agencyId, ManagementEditAgencyRequest request, LocalityInfo localityInfo,
            string languageCode = LocalizationHelper.DefaultLanguageCode);
    }
}