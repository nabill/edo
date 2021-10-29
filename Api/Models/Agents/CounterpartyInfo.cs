using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CounterpartyInfo
    {
        [JsonConstructor]
        public CounterpartyInfo(int id, string name, bool isContractUploaded, string markupFormula = null)
        {
            Id = id;
            Name = name;
            IsContractUploaded = isContractUploaded;
            MarkupFormula = markupFormula;
        }

        /// <summary>
        /// Counterparty Id.
        /// </summary>
        [Required]
        public int Id { get; }

        /// <summary>
        ///     Counterparty name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// True if contract is loaded to counterparty
        /// </summary>
        public bool IsContractUploaded { get; }


        /// <summary>
        /// Displayed markup formula
        /// </summary>
        public string MarkupFormula { get; }
    }
}