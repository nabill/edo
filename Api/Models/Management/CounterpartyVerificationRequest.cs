using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct CounterpartyVerificationRequest
    {
        [JsonConstructor]
        public CounterpartyVerificationRequest(CounterpartyStates state, string reason)
        {
            State = state;
            Reason = reason;
        }


        /// <summary>
        ///     New verification state
        /// </summary>
        public CounterpartyStates State { get; }

        
        /// <summary>
        ///     Verify reason.
        /// </summary>
        public string Reason { get; }
    }
}