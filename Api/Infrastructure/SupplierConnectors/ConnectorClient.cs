using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Infrastructure.SupplierConnectors
{
    public class ConnectorClient : IConnectorClient
    {
        public ConnectorClient(IHttpClientFactory clientFactory, 
            ILogger<ConnectorClient> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _serializer = new JsonSerializer
            {
                Converters = {new ProblemDetailsConverter()}
            };
        }


        public Task<Result<TResponse, ProblemDetails>> Get<TResponse>(Uri url, Dictionary<string, string> customHeaders, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<TResponse>(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                return WithCustomHeaders(request, customHeaders);
            }, languageCode, cancellationToken);


        public Task<Result<TResponse, ProblemDetails>> Post<TRequest, TResponse>(Uri url, TRequest requestContent, Dictionary<string, string> customHeaders, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<TResponse>(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = BuildContent(requestContent)
                };

                return WithCustomHeaders(request, customHeaders);
            }, languageCode, cancellationToken);


        public Task<Result<TResponse, ProblemDetails>> Post<TResponse>(Uri url, Dictionary<string, string> customHeaders, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<TResponse>(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                return WithCustomHeaders(request, customHeaders);
            }, languageCode, cancellationToken);


        public Task<Result<Unit, ProblemDetails>> Post(Uri uri, Dictionary<string, string> customHeaders, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Post<Unit, Unit>(uri, Unit.Instance, customHeaders, languageCode, cancellationToken);


        public Task<Result<TResponse, ProblemDetails>> Post<TResponse>(Uri url, Stream stream, Dictionary<string, string> customHeaders, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<TResponse>(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StreamContent(stream)
                };

                return WithCustomHeaders(request, customHeaders);
            }, languageCode, cancellationToken);


        private static StringContent BuildContent<T>(T requestContent)
            => requestContent is Unit
                ? null
                : new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");


        public async Task<Result<TResponse, ProblemDetails>> Send<TResponse>(Func<HttpRequestMessage> requestFactory, string languageCode,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            try
            {
                var connectorClient = _clientFactory.CreateClient(HttpClientNames.Connectors);
                connectorClient.DefaultRequestHeaders.Add("Accept-Language", languageCode);
                response = await connectorClient.SendAsync(requestFactory(), cancellationToken);

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var streamReader = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(streamReader);

                if (!response.IsSuccessStatusCode)
                {
                    ProblemDetails error;

                    try
                    {
                        error = _serializer.Deserialize<ProblemDetails>(jsonTextReader)
                            ?? ProblemDetailsBuilder.Build($"Connector error is not specified, status code: {response.StatusCode}", response.StatusCode);
                    }
                    catch (JsonReaderException)
                    {
                        streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                        var responseBody = await streamReader.ReadToEndAsync();
                        _logger.LogConnectorClientUnexpectedResponse(response.StatusCode, requestFactory().RequestUri, responseBody);

                        var reasonPhrase = string.IsNullOrWhiteSpace(response.ReasonPhrase)
                            ? $"Connector error is not specified, status code: {response.StatusCode}"
                            : response.ReasonPhrase;

                        error = ProblemDetailsBuilder.Build(reasonPhrase, response.StatusCode);
                    }
                    
                    return Result.Failure<TResponse, ProblemDetails>(error);
                }

                return _serializer.Deserialize<TResponse>(jsonTextReader);
            }
            catch (Exception ex)
            {
                _logger.LogConnectorClientException(requestFactory().RequestUri?.ToString(), await response?.Content?.ReadAsStringAsync(cancellationToken));
                return ProblemDetailsBuilder.Fail<TResponse>(ex.Message);
            }
            finally
            {
                response?.Dispose();
            }
        }


        private static HttpRequestMessage WithCustomHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers is null)
                return request;

            foreach (var (key, value) in headers)
                request.Headers.Add(key, value);

            return request;
        }
       
        
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializer _serializer;
        private readonly ILogger<ConnectorClient> _logger;
    }
}