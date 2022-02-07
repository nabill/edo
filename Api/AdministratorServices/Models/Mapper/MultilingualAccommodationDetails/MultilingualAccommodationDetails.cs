using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using UniqueAccommodationCodes = HappyTravel.MapperContracts.Public.Accommodations.Internals.UniqueAccommodationCodes;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public class MultilingualAccommodationDetails
{
    public MultilingualAccommodationDetails(
        string supplierCode,
        MultiLanguage.MultiLanguage<string> name,
        MultiLanguage.MultiLanguage<List<string>> accommodationAmenities,
        MultiLanguage.MultiLanguage<Dictionary<string, string>> additionalInfo,
        MultiLanguage.MultiLanguage<string> category,
        in ContactDetails contacts,
        in MultilingualLocationDetails location,
        List<ImageDetails> photos,
        AccommodationRatings rating,
        in ScheduleDetails schedule,
        List<MultilingualTextualDescriptionDetails> textualDescriptions,
        PropertyTypes type,
        bool isActive,
        bool hasDirectContract = false,
        UniqueAccommodationCodes? uniqueCodes = null,
        string hotelChain = null)
    {
        SupplierCode = supplierCode;
        AccommodationAmenities = accommodationAmenities;
        AdditionalInfo = additionalInfo;
        Category = category;
        Contacts = contacts;
        Rating = rating;
        Location = location;
        Name = name;
        Photos = photos ?? new List<ImageDetails>(0);
        Schedule = schedule;
        TextualDescriptions = textualDescriptions ?? new List<MultilingualTextualDescriptionDetails>(0);
        Type = type;
        UniqueCodes = uniqueCodes;
        HotelChain = hotelChain;
        IsActive = isActive;
        HasDirectContract = hasDirectContract;
    }
    
    
    public string SupplierCode { get; }
    public MultiLanguage.MultiLanguage<string> Name { get; }
    public MultiLanguage.MultiLanguage<string> Category { get; }
    public ContactDetails Contacts { get; }
    public MultilingualLocationDetails Location { get; }
    public List<ImageDetails> Photos { get; }
    public AccommodationRatings Rating { get; }
    public ScheduleDetails Schedule { get; }
    public List<MultilingualTextualDescriptionDetails> TextualDescriptions { get; }
    public PropertyTypes Type { get; }
    public bool IsActive { get; }
    public UniqueAccommodationCodes? UniqueCodes { get; }
    public string HotelChain { get; }
    public MultiLanguage.MultiLanguage<List<string>> AccommodationAmenities { get; }
    public MultiLanguage.MultiLanguage<Dictionary<string, string>> AdditionalInfo { get; }
    public bool HasDirectContract { get; }
}