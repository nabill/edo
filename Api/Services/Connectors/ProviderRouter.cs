using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class ProviderRouter : IProviderRouter
    {
        public ProviderRouter(IDataProviderFactory dataProviderFactory)
        {
            _dataProviderFactory = dataProviderFactory;
        }


        public async Task<Result<CombinedAvailabilityDetails>> GetAvailability(List<DataProviders> dataProviders, AvailabilityRequest availabilityRequest,
            string languageCode)
        {
            if (dataProviders == null || !dataProviders.Any())
                return Result.Fail<CombinedAvailabilityDetails>($"{nameof(dataProviders)} are required for availability search.");

            var results = await GetResultsFromConnectors();

            var failedResults = results
                .Where(r => r.Result.IsFailure)
                .ToList();

            if (failedResults.Count == results.Count)
            {
                var errorMessage = string.Join("; ", failedResults.Select(r => r.Result.Error.Detail).Distinct());
                return Result.Fail<CombinedAvailabilityDetails>(errorMessage);
            }

            var succeededResults = results
                .Where(r => r.Result.IsSuccess)
                .Select(r => (r.ProviderKey, r.Result.Value))
                .ToList();

            return Result.Ok(CombineAvailabilities(succeededResults));


            async Task<List<(DataProviders ProviderKey, Result<AvailabilityDetails, ProblemDetails> Result)>> GetResultsFromConnectors()
            {
                var getAvailabilityTasks = dataProviders.Select(async providerKey =>
                {
                    var provider = _dataProviderFactory.Get(providerKey);
                    var result = await provider.GetAvailability(availabilityRequest, languageCode);
                    return (providerKey, result);
                }).ToList();

                await Task.WhenAll(getAvailabilityTasks);

                return getAvailabilityTasks
                    .Select(t => t.Result)
                    .ToList();
            }
        }


        public Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> GetAvailable(DataProviders dataProvider, string accommodationId,
            long availabilityId, string languageCode)
        {
            var provider = _dataProviderFactory.Get(dataProvider);
            return provider.GetAvailability(availabilityId, accommodationId, languageCode);
        }


        public Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline, ProblemDetails>> GetExactAvailability(DataProviders dataProvider,
            long availabilityId, Guid agreementId, string languageCode)
        {
            var provider = _dataProviderFactory.Get(dataProvider);
            return provider.GetExactAvailability(availabilityId, agreementId, languageCode);
        }


        public Task<Result<AccommodationDetails, ProblemDetails>> GetAccommodation(DataProviders dataProvider, string id, string languageCode)
        {
            var provider = _dataProviderFactory.Get(dataProvider);
            return provider.GetAccommodation(id, languageCode);
        }


        private CombinedAvailabilityDetails CombineAvailabilities(List<(DataProviders ProviderKey, AvailabilityDetails Availability)> availabilities)
        {
            var firstResult = availabilities.First().Availability;

            var results = availabilities
                .SelectMany(providerResults =>
                {
                    var (providerKey, providerAvailability) = providerResults;
                    var availabilityResults = providerAvailability
                        .Results
                        .Select(r =>
                        {
                            var result = new AvailabilityResult(providerAvailability.AvailabilityId,
                                r.AccommodationDetails,
                                r.Agreements);

                            return ProviderData.Create(providerKey, result);
                        })
                        .ToList();

                    return availabilityResults;
                })
                .ToList();

            var processed = availabilities.Sum(a => a.Availability.NumberOfProcessedAccommodations);
            return new CombinedAvailabilityDetails(firstResult.NumberOfNights, firstResult.CheckInDate, firstResult.CheckOutDate, processed, results);
        }


        private readonly IDataProviderFactory _dataProviderFactory;
    }
}