using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Users;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct RegisterAgentWithAgencyRequest
    {
        [JsonConstructor]
        public RegisterAgentWithAgencyRequest(UserDescriptionInfo agent, RegistrationAgencyInfo agency)
        {
            Agent = agent;
            Agency = agency;
        }


        /// <summary>
        ///     Agent personal information.
        /// </summary>
        public UserDescriptionInfo Agent { get; }

        /// <summary>
        ///     Agent affiliated agency information.
        /// </summary>
        public RegistrationAgencyInfo Agency { get; }
    }
}