using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicySettings
    {
        [JsonConstructor]
        public MarkupPolicySettings(string? description, MarkupFunctionType functionType, decimal value,
            Currencies currency, string? locationScopeId, string? destinationScopeId,
            SubjectMarkupScopeTypes? locationScopeType, DestinationMarkupScopeTypes? destinationScopeType)
        {
            Description = description;
            FunctionType = functionType;
            Value = value;
            Currency = currency;
            LocationScopeId = locationScopeId;
            LocationScopeType = locationScopeType;
            DestinationScopeId = destinationScopeId;
            DestinationScopeType = destinationScopeType;
        }


        public MarkupPolicySettings(string? description, MarkupFunctionType functionType, decimal value,
            Currencies currency, string? locationScopeId = "", SubjectMarkupScopeTypes? locationScopeType = SubjectMarkupScopeTypes.NotSpecified,
            string? destinationScopeId = null, DestinationMarkupScopeTypes? destinationScopeType = DestinationMarkupScopeTypes.NotSpecified)
        {
            Description = description;
            FunctionType = functionType;
            Value = value;
            Currency = currency;
            LocationScopeId = locationScopeId;
            LocationScopeType = locationScopeType;
            DestinationScopeId = destinationScopeId;
            DestinationScopeType = destinationScopeType;
        }


        /// <summary>
        ///     Policy description.
        /// </summary>
        public string? Description { get; }

        public MarkupFunctionType FunctionType { get; }
        public decimal Value { get; }

        /// <summary>
        ///     Currency of policy. Needed for proper currency applying.
        /// </summary>
        public Currencies Currency { get; }


        /// <summary>
        ///     Location of agent from the mapper
        /// </summary>
        public string? LocationScopeId { get; }


        /// <summary>
        ///     Type of location scope of agent from the mapper
        /// </summary>
        public SubjectMarkupScopeTypes? LocationScopeType { get; }


        /// <summary>
        ///     Destination of booking from the mapper
        /// </summary>
        public string? DestinationScopeId { get; }


        /// <summary>
        ///     Destination type of booking from the mapper
        /// </summary>
        public DestinationMarkupScopeTypes? DestinationScopeType { get; }
    }
}