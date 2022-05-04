using HappyTravel.Edo.Common.Enums.Markup;

namespace Api.Models.Markups.Supplier
{
    public readonly struct SupplierMarkupRequest
    {
        /// <summary>
        ///     Policy's description.
        /// </summary>
        public string? Description { get; init; }


        /// <summary>
        ///     Policy's value.
        /// </summary>
        public decimal Value { get; init; }


        /// <summary>
        ///     Location of agent from the mapper
        /// </summary>
        public string? LocationScopeId { get; init; }


        /// <summary>
        ///     Type of location scope of agent from the mapper
        /// </summary>
        public SubjectMarkupScopeTypes? LocationScopeType { get; init; }


        /// <summary>
        ///     Destination of booking from the mapper
        /// </summary>
        public string? DestinationScopeId { get; init; }


        /// <summary>
        ///     Destination type of booking from the mapper
        /// </summary>
        public DestinationMarkupScopeTypes? DestinationScopeType { get; init; }


        /// <summary>
        ///     Supplier code from the mapper
        /// </summary>
        public string? SupplierCode { get; init; }
    }
}