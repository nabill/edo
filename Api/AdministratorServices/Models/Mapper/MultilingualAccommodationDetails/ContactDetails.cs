using System.Collections.Generic;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public class ContactDetails
{
    public List<string> Emails { get; init; }
    public List<string> Faxes { get; init; }
    public List<string> Phones { get; init; }
    public List<string> WebSites { get; init; }
}