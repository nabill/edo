using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AgencyAccountSettings
    {
        [JsonConstructor]
        public AgencyAccountSettings(int agencyId, int agencyAccountId, bool isActive)
        {
            AgencyId = agencyId;
            AgencyAccountId = agencyAccountId;
            IsActive = isActive;
        }


        /// <summary>
        /// Agency Id
        /// </summary>
        public int AgencyId { get; }
        
        /// <summary>
        /// Agency account Id
        /// </summary>
        public int AgencyAccountId { get; }

        /// <summary>
        /// Is the account active
        /// </summary>
        public bool IsActive { get; }
    }
}
