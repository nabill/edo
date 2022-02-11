using HappyTravel.EdoContracts.Accommodations.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public struct PoiDetails
{
    public PoiDetails(string name, double distance, double time, PoiTypes type, string? description = null)
    {
        Name = name;
        Description = description ?? string.Empty;
        Distance = distance;
        Time = time;
        Type = type;
    }

    
    public string Name { get; }
    public string Description { get; }
    public double Distance { get; }
    public double Time { get; }
    public PoiTypes Type { get; }
}