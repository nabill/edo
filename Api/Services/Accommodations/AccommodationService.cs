using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.SuppliersCatalog;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AccommodationService : IAccommodationService
    {
        public AccommodationService(IDoubleFlow flow,
            ISupplierConnectorManager supplierConnectorManager,
            IAccommodationMapperClient mapperClient)
        {
            _flow = flow;
            _supplierConnectorManager = supplierConnectorManager;
            _mapperClient = mapperClient;
        }


        public Task<Result<Accommodation, ProblemDetails>> Get(Suppliers source, string htId, string languageCode)
        {
            return _flow.GetOrSetAsync(_flow.BuildKey(nameof(AccommodationService), nameof(Get), languageCode, htId),
                async () =>
                {
                    var (_, isFailure, accommodation, error) = await _mapperClient.GetAccommodation(htId, languageCode);
                    return isFailure
                        ? ProblemDetailsBuilder.Fail<Accommodation>(error.Detail)
                        : accommodation.ToEdoContract();
                },
                TimeSpan.FromDays(1));
        }


        private readonly IDoubleFlow _flow;
        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IAccommodationMapperClient _mapperClient;
    }
}