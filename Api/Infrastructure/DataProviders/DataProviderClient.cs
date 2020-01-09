using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Infrastructure.DataProviders
{
    public class DataProviderClient : IDataProviderClient
    {
        public DataProviderClient(IHttpClientFactory clientFactory, ILoggerFactory loggerFactory)
        {
            _clientFactory = clientFactory;
            _logger = loggerFactory.CreateLogger<DataProviderClient>();

            _serializer = new JsonSerializer();
        }


        public Task<Result<T, ProblemDetails>> Get<T>(Uri url, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<T>(new HttpRequestMessage(HttpMethod.Get, url), languageCode, cancellationToken);


        public Task<Result<TOut, ProblemDetails>> Post<T, TOut>(Uri url, T requestContent, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<TOut>(new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = BuildContent(requestContent)
            }, languageCode, cancellationToken);


        public Task<Result<VoidObject, ProblemDetails>> Post(Uri uri, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Post<VoidObject, VoidObject>(uri, VoidObject.Instance, languageCode, cancellationToken);


        private static StringContent BuildContent<T>(T requestContent)
            => requestContent is VoidObject
                ? null
                : new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");


        private async Task<Result<TResponse, ProblemDetails>> Send<TResponse>(HttpRequestMessage request, string languageCode,
            CancellationToken cancellationToken)
        {
            try
            {
                using (var client = _clientFactory.CreateClient())
                {
                    client.DefaultRequestHeaders.Add("Accept-Language", languageCode);

                    using (var response = await client.SendAsync(request, cancellationToken))
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            var error = _serializer.Deserialize<ProblemDetails>(jsonTextReader) ??
                                ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                            return Result.Fail<TResponse, ProblemDetails>(error);
                        }

                        var availabilityResponse = _serializer.Deserialize<TResponse>(jsonTextReader);
                        return Result.Ok<TResponse, ProblemDetails>(availabilityResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("requested url", request.RequestUri);

                _logger.LogDataProviderClientException(ex);
                return ProblemDetailsBuilder.Fail<TResponse>(ex.Message);
            }
        }


        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<DataProviderClient> _logger;
        private readonly JsonSerializer _serializer;
    }
}