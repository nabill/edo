using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AgencyAccountEditRequest
    {
        [JsonConstructor]
        public AgencyAccountEditRequest(bool isActive)
        {
            IsActive = isActive;
        }


        /// <summary>
        /// Is the account active
        /// </summary>
        public bool IsActive { get; }
    }
}
