using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AgencyInfo
    {
        [JsonConstructor]
        public AgencyInfo(string title, int? id)
        {
            Title = title;
            Id = id;
        }


        /// <summary>
        ///     Title of the counterparty agency.
        /// </summary>
        [Required]
        public string Title { get; }

        /// <summary>
        ///     Id of the counterparty agency.
        /// </summary>
        public int? Id { get; }
    }
}