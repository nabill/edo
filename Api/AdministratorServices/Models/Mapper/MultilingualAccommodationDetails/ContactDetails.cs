using System.Collections.Generic;

namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public struct ContactDetails
{
    public ContactDetails(
        List<string> emails,
        List<string> phones,
        List<string> webSites,
        List<string> faxes)
    {
        Emails = emails ?? new List<string>(0);
        Faxes = faxes ?? new List<string>(0);
        Phones = phones ?? new List<string>(0);
        WebSites = webSites ?? new List<string>(0);
    }

    
    public List<string> Emails { get; }
    public List<string> Faxes { get; }
    public List<string> Phones { get; }
    public List<string> WebSites { get; }
}