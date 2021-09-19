using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.MapperContracts.Public.Accommodations;
using HappyTravel.SuppliersCatalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.AdministratorServices.AccommodationManagementServices
{
    public class MapperManagementClient : IMapperManagementClient
    {
        public MapperManagementClient(IHttpClientFactory clientFactory, ILogger<MapperManagementClient> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }
        
        
        public Task<Result<Unit, ProblemDetails>> CombineAccommodations(string baseHtAccommodationId, string combinedHtAccommodationId, CancellationToken cancellationToken = default)
        {
            var requestUri = $"api/1.0/AccommodationsManagement/accommodations/combine?{nameof(baseHtAccommodationId)}={baseHtAccommodationId}&{nameof(combinedHtAccommodationId)}={combinedHtAccommodationId}";
            
            return Post(requestUri, null, cancellationToken);
        }


        public Task<Result<Unit, ProblemDetails>> DeactivateAccommodations(AccommodationsRequest request, DeactivationReasons deactivationReason, CancellationToken cancellationToken)
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(new {request.HtAccommodationIds, reason = deactivationReason}), Encoding.UTF8, "application/json");
            var requestUri = "api/1.0/AccommodationsManagement/accommodations/deactivate";
            
            return Post(requestUri, requestContent, cancellationToken);
        }


        public Task<Result<Unit, ProblemDetails>> RemoveSupplier(string htAccommodationId, Suppliers supplier, string supplierId, CancellationToken cancellationToken = default)
        {
            var requestUri = $"api/1.0/AccommodationsManagement/accommodations/{htAccommodationId}/suppliers/{supplier}/accommodations/{supplierId}/remove";
            
            return Post(requestUri, null, cancellationToken);
        }


        private async Task<Result<Unit, ProblemDetails>> Post(string requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            var client = _clientFactory.CreateClient(HttpClientNames.MapperManagement);
            try
            {
                using var response = await client.PostAsync(requestUri, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                    return Result.Success<Unit, ProblemDetails>(Unit.Instance);

                ProblemDetails error;

                try
                {
                    error = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonSerializerOptions, cancellationToken);
                }
                catch (JsonException)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogMapperManagementClientUnexpectedResponse(response.StatusCode, response.RequestMessage?.RequestUri, responseBody);
                    error = ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);
                }

                return Result.Failure<Unit, ProblemDetails>(error);
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogMapperManagementClientRequestTimeout(ex);
                return ProblemDetailsBuilder.Build("Request failure");
            }
            catch (Exception ex)
            {
                _logger.LogMapperManagementClientException(ex);
                return ProblemDetailsBuilder.Build(ex.Message);
            }
        }
        
        
        private static readonly JsonSerializerOptions JsonSerializerOptions = new () 
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters = { new JsonStringEnumConverter() }
        };
        private readonly IHttpClientFactory _clientFactory;
        private readonly  ILogger<MapperManagementClient> _logger;
    }
}