using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct CounterpartyVerificationRequest
    {
        [JsonConstructor]
        public CounterpartyVerificationRequest(string reason)
        {
            Reason = reason;
        }


        /// <summary>
        ///     Verify reason.
        /// </summary>
        public string Reason { get; }
    }
}