using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AgencyInfo
    {
        [JsonConstructor]
        public AgencyInfo(string name, int? id, int? counterpartyId)
        {
            Name = name;
            Id = id;
            CounterpartyId = counterpartyId;
        }


        /// <summary>
        ///     Name of the counterparty agency.
        /// </summary>
        [Required]
        public string Name { get; }

        /// <summary>
        ///     Id of the counterparty agency.
        /// </summary>
        public int? Id { get; }

        /// <summary>
        ///     Id of the counterparty.
        /// </summary>
        public int? CounterpartyId { get; }
    }
}