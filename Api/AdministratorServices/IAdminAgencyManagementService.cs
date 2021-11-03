﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAdminAgencyManagementService
    {
        Task<Result> DeactivateAgency(int agencyId, string reason);

        Task<Result> ActivateAgency(int agencyId, string reason);

        Task<Result<AgencyInfo>> Get(int agencyId, string languageCode = LocalizationHelper.DefaultLanguageCode);

        Task<List<AgencyInfo>> GetChildAgencies(int parentAgencyId, string languageCode = LocalizationHelper.DefaultLanguageCode);

        Task<Result<AgencyInfo>> Create(RegistrationAgencyInfo agencyInfo, int counterpartyId, int? parentAgencyId);
        
        Task<Result<ContractKind>> GetContractKind(int agencyId);

        Task<Result<AgencyVerificationStates>> GetVerificationState(int agencyId);
    }
}
