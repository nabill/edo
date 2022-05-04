using HappyTravel.Edo.Common.Enums.Markup;

namespace Api.Models.Markups.Supplier
{
    public readonly struct SupplierMarkupRequest
    {
        public SupplierMarkupRequest(string description,
            decimal value,
            string destinationScopeId,
            DestinationMarkupScopeTypes destinationMarkupScopeTypes,
            string supplierCode)
        {
            Description = description;
            Value = value;
            DestinationScopeId = destinationScopeId;
            DestinationScopeType = destinationMarkupScopeTypes;
            SupplierCode = supplierCode;
        }


        /// <summary>
        ///     Policy's description.
        /// </summary>
        public string Description { get; init; }


        /// <summary>
        ///     Policy's value.
        /// </summary>
        public decimal Value { get; init; }


        /// <summary>
        ///     Destination of booking from the mapper
        /// </summary>
        public string DestinationScopeId { get; init; }


        /// <summary>
        ///     Destination type of booking from the mapper
        /// </summary>
        public DestinationMarkupScopeTypes DestinationScopeType { get; init; }


        /// <summary>
        ///     Supplier code from the mapper
        /// </summary>
        public string SupplierCode { get; init; }
    }
}