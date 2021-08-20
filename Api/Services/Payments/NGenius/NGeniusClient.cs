using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class NGeniusClient
    {
        public NGeniusClient(IOptions<NGeniusOptions> options, IHttpClientFactory clientFactory, IMemoryFlow<string> cache)
        {
            _options = options.Value;
            _cache = cache;
            _clientFactory = clientFactory;
        }


        public async Task<Result<NGeniusPaymentResponse>> Authorize(OrderRequest order)
        {
            var endpoint = $"transactions/outlets/{_options.OutletId}/payment/card";
            var response = await Post(endpoint, order);

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            if (!response.IsSuccessStatusCode)
                return Result.Failure<NGeniusPaymentResponse>(GetErrorMessage(document));

            var paymentId = GetStringValue(document.RootElement, "orderReference");
            var merchantOrderReference = GetStringValue(document.RootElement, "merchantOrderReference");
            var paymentInformation = GetResponsePaymentInformation(document);
            var status = MapToStatus(GetStringValue(document.RootElement, "state"));

            Secure3dOptions? secure3dOptions = status == CreditCardPaymentStatuses.Secure3d
                ? GetSecure3dOptions(document)
                : null;

            return new NGeniusPaymentResponse(paymentId: paymentId,
                status: status, 
                merchantOrderReference: merchantOrderReference, 
                payment: paymentInformation,
                secure3dOptions: secure3dOptions);
        }
        
        
        private async Task<string> GetAccessToken()
        {
            var key = _cache.BuildKey(nameof(NGeniusClient), "access-token");

            if (_cache.TryGetValue<string>(key, out var token))
                return token;

            using var client = _clientFactory.CreateClient(HttpClientNames.NGenius);
            var request = new HttpRequestMessage(HttpMethod.Post, "identity/auth/access-token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _options.Token);
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var data = JsonSerializer.Deserialize<NGeniusAuthResponse>(await response.Content.ReadAsStringAsync());
            _cache.Set(key, data.AccessToken, TimeSpan.FromSeconds(data.ExpiresIn).Subtract(TimeSpan.FromMinutes(1)));
            token = data.AccessToken;

            return token;
        }


        private async Task<HttpResponseMessage> Post<T>(string endpoint, T data)
        {
            var token = await GetAccessToken();
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(data, SerializerOptions), null, "application/vnd.ni-payment.v2+json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var client = _clientFactory.CreateClient(HttpClientNames.NGenius);
            return await client.SendAsync(request);
        }


        private static string GetErrorMessage(JsonDocument document)
        {
            var messages = document.RootElement.GetProperty("errors")
                .EnumerateArray()
                .Select(e => e.GetProperty("localizedMessage").GetString())
                .ToList();
            
            return string.Join(';', messages);
        }


        private static ResponsePaymentInformation GetResponsePaymentInformation(JsonDocument document)
        {
            var element = document.RootElement.GetProperty("paymentMethod");
            return new ResponsePaymentInformation(pan: GetStringValue(element, "pan"),
                expiry: GetStringValue(element, "expiry"),
                cvv: GetStringValue(element, "cvv"),
                cardholderName: GetStringValue(element, "cardholderName"),
                name: GetStringValue(element, "name"));
        }


        private static Secure3dOptions GetSecure3dOptions(JsonDocument document)
        {
            var element = document.RootElement.GetProperty("3ds");
            return new Secure3dOptions(acsUrl: GetStringValue(element, "acsUrl"), 
                acsPaReq: GetStringValue(element, "acsPaReq"),
                acsMd: GetStringValue(element, "acsMd"));
        }


        private static CreditCardPaymentStatuses MapToStatus(string state)
        {
            return state switch
            {
                StateTypes.Authorized => CreditCardPaymentStatuses.Success,
                StateTypes.Await3Ds => CreditCardPaymentStatuses.Secure3d,
                StateTypes.Failed => CreditCardPaymentStatuses.Failed,
                _ => throw new NotSupportedException($"Payment status `{state}` not supported")
            };
        }


        private static string GetStringValue(JsonElement element, string key) 
            => element.GetProperty(key).GetString();


        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };


        private readonly NGeniusOptions _options;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryFlow<string> _cache;
    }
}