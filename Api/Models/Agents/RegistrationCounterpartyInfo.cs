using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct RegistrationCounterpartyInfo
    {
        [JsonConstructor]
        public RegistrationCounterpartyInfo(string name, string localityHtId = null)
        {
            Name = name;
            LocalityHtId = localityHtId;
        }

        /// <summary>
        ///     Counterparty name.
        /// </summary>
        [Required]
        public string Name { get; }

        /// <summary>
        /// Locality of counterparty
        /// </summary>
        public string LocalityHtId { get; }
    }
}