namespace HappyTravel.Edo.Api.AdministratorServices.Models.Mapper
{
    public readonly struct CountryData
    {
        public CountryData(int id, string countryName)
        {
            Id = id;
            CountryName = countryName ?? string.Empty;
        }


        public int Id { get; init; }
        public string CountryName { get; init; }
    }
}