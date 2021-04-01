using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgencyService : IAgencyService
    {
        public AgencyService(IAdminAgencyManagementService adminAgencyManagementService)
        {
            _adminAgencyManagementService = adminAgencyManagementService;
        }


        public Task<Result<SlimAgencyInfo>> Get(AgentContext agent, string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            return _adminAgencyManagementService.Get(agent.AgencyId, languageCode)
                .Map(a => new SlimAgencyInfo(
                    a.Name,
                    a.Address,
                    a.BillingEmail,
                    a.City,
                    a.CountryCode,
                    a.CountryName,
                    a.Fax,
                    a.Phone,
                    a.PostalCode,
                    a.Website,
                    a.VatNumber));
        }


        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
    }
}
