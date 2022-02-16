namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;

public class SupplierAccommodation
{
    public string SupplierCode { get; init; } = string.Empty;
    public AccommodationData Data { get; set; } = new();
}