using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.MultiLanguage;
using Microsoft.AspNetCore.Mvc;
using ApiModels = HappyTravel.Edo.Api.Models.Locations;
using MapperModels = HappyTravel.MapperContracts.Public.Locations;

namespace Api.AdministratorServices.Locations
{
    public class CountryManagementService : ICountryManagementService
    {
        public CountryManagementService(ICountryManagementStorage countryStorage,
            IMapperManagementClient mapperManagementClient)
        {
            _countryStorage = countryStorage;
            _mapperManagementClient = mapperManagementClient;
        }


        public async Task<List<ApiModels.Country>> Get(CancellationToken cancellationToken)
        {
            var countries = await _countryStorage.Get(cancellationToken);
            return countries
                .Select(ToApiProjection())
                .ToList();
        }


        public async Task<Result<List<ApiModels.Country>, ProblemDetails>> Actualize(string language, CancellationToken cancellationToken)
        {
            var edoCountries = await _countryStorage.Get(cancellationToken);

            var (_, isFailure, mapperCountries, error) = await _mapperManagementClient.GetAllCountries(language, cancellationToken);
            if (isFailure)
                return Result.Failure<List<ApiModels.Country>, ProblemDetails>(error);

            var difference = mapperCountries
                .Select(ToDataProjection())
                .Except(edoCountries, new CountryComparer())
                .ToList();

            await _countryStorage.UpdateRange(difference, cancellationToken);
            await _countryStorage.Refresh(cancellationToken);

            return difference
                .Select(ToApiProjection())
                .ToList();


            Func<MapperModels.SlimCountry, Country> ToDataProjection()
                => slimCountry => new Country()
                {
                    Code = slimCountry.Code,
                    Names = new MultiLanguage<string>() { En = slimCountry.Name },
                    RegionId = null,
                    MarketId = UnknownMarketId
                };
        }


        private class CountryComparer : IEqualityComparer<Country>
        {
            public bool Equals(Country? first, Country? second)
            {
                if (first is null || second is null)
                    return false;

                return first.Code == second.Code;
            }

            public int GetHashCode(Country obj) => obj.Code.GetHashCode();
        }


        private Func<Country, ApiModels.Country> ToApiProjection()
            => country => new ApiModels.Country(country.Code,
                country.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode),
                country.MarketId,
                country.RegionId);


        private const int UnknownMarketId = 1;

        private readonly ICountryManagementStorage _countryStorage;
        private readonly IMapperManagementClient _mapperManagementClient;
    }
}