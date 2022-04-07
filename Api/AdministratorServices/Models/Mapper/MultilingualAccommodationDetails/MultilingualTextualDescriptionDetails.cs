using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public record MultilingualTextualDescriptionDetails
{
    public MultiLanguage.MultiLanguage<string> Description { get; init; }
    public TextualDescriptionTypes Type { get; init; }
}