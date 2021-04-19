using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AgencyAccountSettings
    {
        [JsonConstructor]
        public AgencyAccountSettings(bool isActive)
        {
            IsActive = isActive;
        }


        /// <summary>
        /// Is the account active
        /// </summary>
        public bool IsActive { get; }
    }
}
