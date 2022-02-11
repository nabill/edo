namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper.MultilingualAccommodationDetails;

public struct ImageDetails
{
    public ImageDetails(string sourceUrl, string caption)
    {
        Caption = caption;
        SourceUrl = sourceUrl;
    }

    
    public string Caption { get; }
    public string SourceUrl { get; }
}