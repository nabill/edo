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


        public async Task<Result<NGeniusPaymentResponse>> CreateOrder(OrderRequest order)
        {
            var endpoint = $"transactions/outlets/{_options.OutletId}/payment/card";
            var response = await Send(HttpMethod.Post, endpoint, order);

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success(ParseResponseInformation(document))
                : Result.Failure<NGeniusPaymentResponse>(ParseErrorMessage(document));
        }


        public async Task<Result<CreditCardPaymentStatuses>> SubmitPaRes(string paymentId, string orderReference, NGenius3DSecureData data)
        {
            var endpoint = $"transactions/outlets/{_options.OutletId}/orders/{orderReference}/payments/{paymentId}/card/3ds";
            var response = await Send(HttpMethod.Post, endpoint, data);
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success(MapToStatus(GetStringValue(document.RootElement, "state")))
                : Result.Failure<CreditCardPaymentStatuses>(ParseErrorMessage(document));
        }


        public async Task<Result<string>> CaptureMoney(string paymentId, string orderReference, NGeniusAmount amount)
        {
            var endpoint = $"transactions/outlets/{_options.OutletId}/orders/{orderReference}/payments/{paymentId}/captures";
            var response = await Send(HttpMethod.Post, endpoint, new { Amount = amount });
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success(ParseCaptureId(document))
                : Result.Failure<string>(ParseErrorMessage(document));
        }


        public async Task<Result> VoidMoney(string paymentId, string orderReference)
        {
            var endpoint = $"transactions/outlets/{_options.OutletId}/orders/{orderReference}/payments/{paymentId}/cancel";
            var response = await Send(HttpMethod.Put, endpoint);
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(ParseErrorMessage(document));
        }
        
        
        public async Task<Result> RefundMoney(string paymentId, string orderReference, string captureId, NGeniusAmount amount)
        {
            var endpoint = $"transactions/outlets/{_options.OutletId}/orders/{orderReference}/payments/{paymentId}/captures/{captureId}/refund";
            var response = await Send(HttpMethod.Post, endpoint, new { Amount = amount });
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(ParseErrorMessage(document));
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


        private async Task<HttpResponseMessage> Send<T>(HttpMethod method, string endpoint, T data)
        {
            var token = await GetAccessToken();
            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            if (data is not null)
                request.Content = new StringContent(JsonSerializer.Serialize(data, SerializerOptions), null, "application/vnd.ni-payment.v2+json");
            
            using var client = _clientFactory.CreateClient(HttpClientNames.NGenius);
            return await client.SendAsync(request);
        }
        
        
        private async Task<HttpResponseMessage> Send(HttpMethod method, string endpoint)
        {
            var token = await GetAccessToken();
            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var client = _clientFactory.CreateClient(HttpClientNames.NGenius);
            return await client.SendAsync(request);
        }


        private static string ParseErrorMessage(in JsonDocument document)
        {
            var element = document.RootElement.GetProperty("errors");
            var count = element.GetArrayLength();
            var messages = new string[count];

            for (var i = 0; i < count; i++)
                messages[i] = GetStringValue(element[i], "localizedMessage");

            return string.Join(';', messages);
        }


        private static NGeniusPaymentResponse ParseResponseInformation(in JsonDocument document)
        {
            var rootElement = document.RootElement;
            var status = MapToStatus(GetStringValue(rootElement, "state"));
            
            return new NGeniusPaymentResponse(paymentId: GetStringValue(rootElement, "_id").Split(':').Last(),
                captureId: ParseCaptureId(document),
                status: status, 
                orderReference: GetStringValue(rootElement, "orderReference"),
                merchantOrderReference: GetStringValue(rootElement, "merchantOrderReference"), 
                payment: ParseResponsePaymentInformation(document),
                secure3dOptions: status == CreditCardPaymentStatuses.Secure3d
                    ? ParseSecure3dOptions(document)
                    : null);
        }


        private static ResponsePaymentInformation ParseResponsePaymentInformation(in JsonDocument document)
        {
            var element = document.RootElement.GetProperty("paymentMethod");
            return new ResponsePaymentInformation(pan: GetStringValue(element, "pan"),
                expiry: GetStringValue(element, "expiry"),
                cvv: GetStringValue(element, "cvv"),
                cardholderName: GetStringValue(element, "cardholderName"),
                name: GetStringValue(element, "name"));
        }


        private static Secure3dOptions ParseSecure3dOptions(in JsonDocument document)
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
                StateTypes.Captured => CreditCardPaymentStatuses.Success,
                _ => throw new NotSupportedException($"Payment status `{state}` not supported")
            };
        }


        private static string GetStringValue(in JsonElement element, string key) 
            => element.GetProperty(key).GetString();


        private static string ParseCaptureId(in JsonDocument document)
        {
            if (!document.RootElement.TryGetProperty("_embedded", out var embeddedElement))
                return null;

            if (!embeddedElement.TryGetProperty("cnp:capture", out var captureElement))
                return null;
            
            // From NGenius documentation:
            // Note that 'index' in this context will always be 0 (zero) unless the order represents a recurring payment.
            return captureElement[0]
                .GetProperty("_links")
                .GetProperty("self")
                .GetProperty("href")
                .GetString()?
                .Split('/')
                .LastOrDefault();
        }


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