using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Deadline
{
    public class DeadlineService : IDeadlineService
    {
        public DeadlineService(IDataProviderClient dataProviderClient,
            IOptions<DataProviderOptions> options,
            IMemoryFlow flow)
        {
            _dataProviderClient = dataProviderClient;
            _options = options.Value;
            _flow = flow;
        }

        public async Task<Result<DeadlineDetails, ProblemDetails>> GetDeadlineDetails(DeadlineDetailsRequest deadlineRequest)
        {
            var cacheKey = _flow.BuildKey(deadlineRequest.ContractType.ToString(),
                deadlineRequest.AccommodationId, deadlineRequest.AvailabilityId, deadlineRequest.TariffCode);
            if (_flow.TryGetValue(cacheKey, out DeadlineDetails result))
                return Result.Ok<DeadlineDetails, ProblemDetails>(result);

            Result<DeadlineDetails, ProblemDetails> response;
            switch (deadlineRequest.ContractType)
            {
                case DataProvidersContractTypes.Netstorming:
                    {
                        response = await GetDeadlineDetailsFromNetstorming(
                            deadlineRequest.AccommodationId,
                            deadlineRequest.AvailabilityId,
                            deadlineRequest.TariffCode,
                            deadlineRequest.LanguageCode
                            );
                        break;
                    }
                case DataProvidersContractTypes.Direct:
                case DataProvidersContractTypes.Illusions:
                    return ProblemDetailsBuilder.Fail<DeadlineDetails>($"{nameof(deadlineRequest.ContractType)}:{deadlineRequest.ContractType} hasn't implemented yet");
                default: return ProblemDetailsBuilder.Fail<DeadlineDetails>("Unknow contract type");
            }
            if (response.IsSuccess)
            {
                _flow.Set(cacheKey, response.Value, _expirationPeriod);
            }
            return response;
        }

        Task<Result<DeadlineDetails, ProblemDetails>> GetDeadlineDetailsFromNetstorming(
            string accommodationId, string availabilityId, string agreemeentCode, string languageCode)
        {
            var uri = new Uri($"{_options.Netstorming}hotels/{accommodationId}/deadline/{availabilityId}/{agreemeentCode}", UriKind.Absolute);
            return _dataProviderClient.Get<DeadlineDetails>(uri, languageCode);
        }

        private readonly TimeSpan _expirationPeriod = TimeSpan.FromHours(1);
        private readonly IMemoryFlow _flow;
        private readonly DataProviderOptions _options;
        private readonly IDataProviderClient _dataProviderClient;
    }
}
