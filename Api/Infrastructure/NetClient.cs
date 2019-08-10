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

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class NetClient : INetClient
    {
        public NetClient(IHttpClientFactory clientFactory, ILoggerFactory loggerFactory)
        {
            _clientFactory = clientFactory;
            _logger = loggerFactory.CreateLogger<NetClient>();

            _serializer = new JsonSerializer();
        }


        public Task<Result<T, ProblemDetails>> Get<T>(Uri url, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<T>(new HttpRequestMessage(HttpMethod.Get, url), languageCode, cancellationToken);


        public Task<Result<TOut, ProblemDetails>> Post<T, TOut>(Uri url, T requestContent, string languageCode = LocalizationHelper.DefaultLanguageCode, CancellationToken cancellationToken = default)
            => Send<TOut>(new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = BuildContent(requestContent)
            }, languageCode, cancellationToken);


        private static StringContent BuildContent<T>(T requestContent) 
            => new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");


        private async Task<Result<T,ProblemDetails>> Send<T>(HttpRequestMessage request, string languageCode, CancellationToken cancellationToken)
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
                            var error = _serializer.Deserialize<ProblemDetails>(jsonTextReader);
                            return Result.Fail<T, ProblemDetails>(error);
                        }

                        var availabilityResponse = _serializer.Deserialize<T>(jsonTextReader);
                        return Result.Ok<T, ProblemDetails>(availabilityResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add("requested url", request.RequestUri);

                _logger.LogNetClientException(ex);
                return ProblemDetailsBuilder.BuildFailResult<T>(ex.Message);
            }
        }
    
        
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializer _serializer;
        private readonly ILogger<NetClient> _logger;
    }
}
