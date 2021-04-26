using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Counterparties
{
    public readonly struct CounterpartyAccountEditRequest
    {
        [JsonConstructor]
        public CounterpartyAccountEditRequest(bool isActive)
        {
            IsActive = isActive;
        }


        /// <summary>
        /// Is the account active
        /// </summary>
        public bool IsActive { get; }
    }
}
