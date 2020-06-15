using System;
using System.ComponentModel.Design;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Infrastructure.DataProviders
{
    public class DataProviderClient : IDataProviderClient
    {
        public DataProviderClient(IHttpClientFactory clientFactory, IOptions<ClientCredentialsTokenRequest> tokenRequestOptions,
            ILogger<DataProviderClient> logger, IHttpContextAccessor httpContextAccessor, IDateTimeProvider dateTimeProvider)
        {
            _clientFactory = clientFactory;
            _tokenRequest = tokenRequestOptions.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _serializer = new JsonSerializer();
            _dateTimeProvider = dateTimeProvider;
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


        public Task<Result<TOut, ProblemDetails>> Post<TOut>(Uri url, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<TOut>(new HttpRequestMessage(HttpMethod.Post, url), languageCode, cancellationToken);


        public Task<Result<VoidObject, ProblemDetails>> Post(Uri uri, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Post<VoidObject, VoidObject>(uri, VoidObject.Instance, languageCode, cancellationToken);


        public Task<Result<TOut, ProblemDetails>> Post<TOut>(Uri url, Stream stream, string languageCode = LocalizationHelper.DefaultLanguageCode,
            CancellationToken cancellationToken = default)
            => Send<TOut>(new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StreamContent(stream)
            }, languageCode, cancellationToken);


        private static StringContent BuildContent<T>(T requestContent)
            => requestContent is VoidObject
                ? null
                : new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");


        public async Task<Result<TResponse, ProblemDetails>> Send<TResponse>(HttpRequestMessage request, string languageCode,
            CancellationToken cancellationToken)
        {
            try
            {
                using var client = _clientFactory.CreateClient();

                client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
                var (_, isFailure, token, getTokenError) = await GetToken();
                if (isFailure)
                    return Result.Failure<TResponse, ProblemDetails>(getTokenError);

                request.SetBearerToken(token);
                using var response = await client.SendAsync(request, cancellationToken);
                await using var stream = await response.Content.ReadAsStreamAsync();
                using var streamReader = new StreamReader(stream);
                using var jsonTextReader = new JsonTextReader(streamReader);

                if (!response.IsSuccessStatusCode)
                {
                    var error = _serializer.Deserialize<ProblemDetails>(jsonTextReader) ??
                        ProblemDetailsBuilder.Build(response.ReasonPhrase, response.StatusCode);

                    return Result.Failure<TResponse, ProblemDetails>(error);
                }

                var availabilityResponse = _serializer.Deserialize<TResponse>(jsonTextReader);
                return Result.Ok<TResponse, ProblemDetails>(availabilityResponse);
            }
            catch (Exception ex)
            {
                ex.Data.Add("requested url", request.RequestUri);

                _logger.LogError(ex, "Http request failed");
                return ProblemDetailsBuilder.Fail<TResponse>(ex.Message);
            }
        }


        private async Task<Result<string, ProblemDetails>> GetToken()
        {
            await TokenSemaphore.WaitAsync();
            var now = _dateTimeProvider.UtcNow();
            // We need to cache token because users can send several requests in short periods.
            // Covered situation when after checking expireDate token will expire immediately.
            if (!_tokenInfo.Equals(default) && (_tokenInfo.ExpiryDate - now).TotalSeconds >= 5)
            {
                TokenSemaphore.Release();
                return _tokenInfo.Token;
            }

            using var client = _clientFactory.CreateClient(HttpClientNames.Identity);

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(_tokenRequest);
            if (tokenResponse.IsError)
            {
                var errorMessage = $"Something went wrong while requesting the access token. Error: {tokenResponse.Error}";
                _logger.LogError(errorMessage);
                Result.Failure<string, ProblemDetails>(
                    ProblemDetailsBuilder.Build(errorMessage));
            }

            _tokenInfo = (tokenResponse.AccessToken, now.AddSeconds(tokenResponse.ExpiresIn));

            TokenSemaphore.Release();
            return Result.Ok<string, ProblemDetails>(_tokenInfo.Token);
        }


        private static (string Token, DateTime ExpiryDate) _tokenInfo;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ClientCredentialsTokenRequest _tokenRequest;
        private readonly JsonSerializer _serializer;
        private readonly ILogger<DataProviderClient> _logger;
        private static readonly SemaphoreSlim TokenSemaphore = new SemaphoreSlim(1, 1);
    }
}