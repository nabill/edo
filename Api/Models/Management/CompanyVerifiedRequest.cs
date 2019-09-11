using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct CompanyVerifiedRequest
    {
        [JsonConstructor]
        public CompanyVerifiedRequest(int companyId, string reason)
        {
            CompanyId = companyId;
            Reason = reason;
        }
        
        /// <summary>
        /// Id of the company to verify.
        /// </summary>
        public int CompanyId { get; }
        
        /// <summary>
        /// Verify reason.
        /// </summary>
        public string Reason { get; }
    }
}