using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerUserSettings
    {
        [JsonConstructor]
        public CustomerUserSettings(bool applyEndClientMarkups)
        {
            ApplyEndClientMarkups = applyEndClientMarkups;
        }
        
        /// <summary>
        /// Apply end-client markups to search results and booking.
        /// </summary>
        public bool ApplyEndClientMarkups { get; }
    }
}