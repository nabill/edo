using HappyTravel.MapperContracts.Internal.Mappings.Enums;

namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct SlimLocationDescription
    {
        public string Name { get; init; }
        public MapperLocationTypes Type { get; init; }
    }
}