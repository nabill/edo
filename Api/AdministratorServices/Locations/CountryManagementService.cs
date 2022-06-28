using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Data.Locations;
using ApiModels = HappyTravel.Edo.Api.Models.Locations;

namespace Api.AdministratorServices.Locations
{
    public class CountryManagementService : ICountryManagementService
    {
        public CountryManagementService(ICountryManagementStorage countryStorage)
        {
            _countryStorage = countryStorage;
        }


        public async Task<List<ApiModels.Country>> Get(CancellationToken cancellationToken)
        {
            var countries = await _countryStorage.Get(cancellationToken);
            return countries
                .Select(ToApiProjection())
                .ToList();


            Func<Country, ApiModels.Country> ToApiProjection()
                => country => new ApiModels.Country(country.Code,
                    country.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode),
                    country.MarketId,
                    country.RegionId);
        }


        private readonly ICountryManagementStorage _countryStorage;
    }
}