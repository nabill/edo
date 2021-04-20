using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AgencyAccountRequest
    {
        [JsonConstructor]
        public AgencyAccountRequest(bool isActive)
        {
            IsActive = isActive;
        }


        /// <summary>
        /// Is the account active
        /// </summary>
        public bool IsActive { get; }
    }
}
