using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using UniqueAccommodationCodes = HappyTravel.MapperContracts.Public.Accommodations.Internals.UniqueAccommodationCodes;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public class MultilingualAccommodationDetails
{
    public string? SupplierCode { get; init; }
    public MultiLanguage.MultiLanguage<string>? Name { get; init; }
    public MultiLanguage.MultiLanguage<string>? Category { get; init; }
    public ContactDetails Contacts { get; init; }
    public MultilingualLocationDetails Location { get; init; }
    public List<ImageDetails> Photos { get; init; } = new();
    public AccommodationRatings Rating { get; init; }
    public ScheduleDetails Schedule { get; init; }
    public List<MultilingualTextualDescriptionDetails> TextualDescriptions { get; init; } = new();
    public PropertyTypes Type { get; init; }
    public bool IsActive { get; init; }
    public UniqueAccommodationCodes? UniqueCodes { get; init; }
    public string? HotelChain { get; init; }
    public MultiLanguage.MultiLanguage<List<string>> AccommodationAmenities { get; init; } = new();
    public MultiLanguage.MultiLanguage<Dictionary<string, string>> AdditionalInfo { get; init; } = new();
    public bool HasDirectContract { get; init; }
}