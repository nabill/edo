namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct LocalityData
    {
        public LocalityData(int id, int countryId, string localityName)
        {
            Id = id;
            CountryId = countryId;
            LocalityName = localityName ?? string.Empty;
        }


        public int Id { get; init; }
        public int CountryId { get; init; }
        public string LocalityName { get; init; }
    }
}