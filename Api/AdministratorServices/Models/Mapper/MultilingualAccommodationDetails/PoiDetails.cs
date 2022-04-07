using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public struct PoiDetails
{
    public string? Name { get; init; }
    public string? Description { get; init;}
    public double Distance { get; init;}
    public double Time { get; init;}
    public PoiTypes Type { get; init;}
}