using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CounterpartyEditRequest
    {
        [JsonConstructor]
        public CounterpartyEditRequest(string name)
        {
            Name = name;
        }


        /// <summary>
        ///     Counterparty name.
        /// </summary>
        [Required]
        public string Name { get; }
    }
}