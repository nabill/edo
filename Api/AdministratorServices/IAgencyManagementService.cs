using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyManagementService
    {
        Task<Result> DeactivateAgency(int agencyId, string reason);

        Task<Result> ActivateAgency(int agencyId, string reason);

        Task<Result<AgencyInfo>> Get(int agencyId);

        Task<List<AgencyInfo>> GetChildAgencies(int parentAgencyId);

        Task<AgencyInfo> Create(string name, int counterpartyId, int? parentId);
    }
}
