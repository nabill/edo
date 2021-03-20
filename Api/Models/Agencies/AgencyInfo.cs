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


        public override int GetHashCode()
            => (Name, Id, CounterpartyId).GetHashCode();


        public bool Equals(AgencyInfo other)
            => (Name, Id, CounterpartyId) == (other.Name, other.Id, other.CounterpartyId);


        public override bool Equals(object obj)
            => obj is AgencyInfo other && Equals(other);
    }
}