using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicySettings
    {
        [JsonConstructor]
        public MarkupPolicySettings(string description, MarkupFunctionType functionType, decimal value, 
            Currencies currency, string locationScopeId = "", string destinationScopeId = "")
        {
            Description = description;
            FunctionType = functionType;
            Value = value;
            Currency = currency;
            LocationScopeId = locationScopeId;
            DestinationScopeId = destinationScopeId;
        }


        /// <summary>
        ///     Policy description.
        /// </summary>
        public string Description { get; }

        public MarkupFunctionType FunctionType { get; }
        public decimal Value { get; }

        /// <summary>
        ///     Currency of policy. Needed for proper currency applying.
        /// </summary>
        public Currencies Currency { get; }
        
        
        /// <summary>
        ///     Location of agent from the mapper
        /// </summary>
        public string LocationScopeId { get; }
        
        /// <summary>
        ///     Destination of booking from the mapper
        /// </summary>
        public string DestinationScopeId { get; }
    }
}