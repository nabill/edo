using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class MultiProviderAvailabilityManager : IMultiProviderAvailabilityManager
    {
        public MultiProviderAvailabilityManager(IDataProviderFactory dataProviderFactory)
        {
            _dataProviderFactory = dataProviderFactory;
        }


        public async Task<Result<AvailabilityDetails>> GetAvailability(AvailabilityRequest availabilityRequest, string languageCode)
        {
            var results = await GetResultsFromConnectors();

            var failedResults = results
                .Where(r => r.Result.IsFailure)
                .ToList();

            if (failedResults.Count == results.Count)
            {
                var errorMessage = string.Join("; ", failedResults.Select(r => r.Result.Error.Detail).Distinct());
                return Result.Fail<AvailabilityDetails>(errorMessage);
            }

            var succeededResults = results
                .Where(r => r.Result.IsSuccess)
                .Select(r=> (r.ProviderKey, r.Result.Value))
                .ToList();

            return Result.Ok(CombineAvailabilities(succeededResults));

            
            async Task<List<(DataProviders ProviderKey, Result<AvailabilityDetails, ProblemDetails> Result)>> GetResultsFromConnectors()
            {
                var getAvailabilityTasks = _dataProviderFactory
                    .GetAll()
                    .Select(async providerInfo =>
                    {
                        var result = await providerInfo.Provider.GetAvailability(availabilityRequest, languageCode);
                        return (providerInfo.Key, result);
                    })
                    .ToList();
                    
                await Task.WhenAll(getAvailabilityTasks);

                return getAvailabilityTasks
                    .Select(t => t.Result)
                    .ToList();
            }
        }


        private AvailabilityDetails CombineAvailabilities(List<(DataProviders ProviderKey, AvailabilityDetails Availability)> availabilities)
        {
            // TODO: implement results combination and new model for combined availability.
            var firstResult = availabilities.First().Availability;
            return firstResult;
        }

        private readonly IDataProviderFactory _dataProviderFactory;
    }
}