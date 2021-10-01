using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments.NGenius;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public static class AgentAgencyExtensions
    {
        public static NGeniusBillingAddress ToBillingAddress(this (AgentContext Agent, SlimAgencyInfo Agency) agentAgencyTuple) 
            => new ()
            {
                FirstName = agentAgencyTuple.Agent.FirstName,
                LastName = agentAgencyTuple.Agent.LastName,
                Address1 = agentAgencyTuple.Agency.Address,
                City = agentAgencyTuple.Agency.City,
                CountryCode = agentAgencyTuple.Agency.CountryCode
            };
    }
}