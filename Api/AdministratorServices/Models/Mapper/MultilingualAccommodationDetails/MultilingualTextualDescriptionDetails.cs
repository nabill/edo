using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public struct MultilingualTextualDescriptionDetails
{
    public MultilingualTextualDescriptionDetails(TextualDescriptionTypes type, MultiLanguage.MultiLanguage<string> description)
    {
        Type = type;
        Description = description;
    }

    
    public MultiLanguage.MultiLanguage<string> Description { get; }
    public TextualDescriptionTypes Type { get; }
}