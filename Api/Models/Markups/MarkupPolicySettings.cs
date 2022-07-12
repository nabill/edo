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
            SubjectMarkupScopeTypes? locationScopeType, DestinationMarkupScopeTypes? destinationScopeType,
            string? supplierCode = default)
        {
            Description = description;
            FunctionType = functionType;
            Value = value;
            Currency = currency;
            LocationScopeId = locationScopeId;
            LocationScopeName = null;
            LocationScopeType = locationScopeType;
            DestinationScopeId = destinationScopeId;
            DestinationScopeName = null;
            DestinationScopeType = destinationScopeType;
            SupplierCode = supplierCode;
        }


        public MarkupPolicySettings(string? description, MarkupFunctionType functionType, decimal value,
                Currencies currency, string? locationScopeId, string? supplierCode = default)
        {
            Description = description;
            FunctionType = functionType;
            Value = value;
            Currency = currency;
            LocationScopeId = locationScopeId ?? string.Empty;
            LocationScopeName = null;
            LocationScopeType = SubjectMarkupScopeTypes.Global;
            DestinationScopeId = string.Empty;
            DestinationScopeName = null;
            DestinationScopeType = DestinationMarkupScopeTypes.Global;
            SupplierCode = supplierCode;
        }


        public MarkupPolicySettings(MarkupFunctionType functionType, decimal value,
            Currencies currency, string description, string locationScopeId, string? locationScopeName,
            SubjectMarkupScopeTypes locationScopeType, string destinationScopeId, string? destinationScopeName,
            DestinationMarkupScopeTypes destinationScopeType, string? supplierCode)
        {
            Description = description;
            FunctionType = functionType;
            Value = value;
            Currency = currency;
            LocationScopeId = locationScopeId;
            LocationScopeName = locationScopeName;
            LocationScopeType = locationScopeType;
            DestinationScopeId = destinationScopeId;
            DestinationScopeName = destinationScopeName;
            DestinationScopeType = destinationScopeType;
            SupplierCode = supplierCode;
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
        ///     Location name of agent from the mapper
        /// </summary>
        public string? LocationScopeName { get; }

        /// <summary>
        ///     Type of location scope of agent from the mapper
        /// </summary>
        public SubjectMarkupScopeTypes? LocationScopeType { get; }

        /// <summary>
        ///     Destination of booking from the mapper
        /// </summary>
        public string? DestinationScopeId { get; }

        /// <summary>
        ///     Destination name of booking from the mapper
        /// </summary>
        public string? DestinationScopeName { get; }

        /// <summary>
        ///     Destination type of booking from the mapper
        /// </summary>
        public DestinationMarkupScopeTypes? DestinationScopeType { get; }

        /// <summary>
        ///     Destination type of booking from the mapper
        /// </summary>
        public string? SupplierCode { get; }
    }
}