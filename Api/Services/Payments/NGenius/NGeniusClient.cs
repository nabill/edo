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
using HappyTravel.MailSender.Infrastructure;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class NGeniusClient : INGeniusClient
    {
        public NGeniusClient(IOptions<NGeniusOptions> options, IHttpClientFactory clientFactory, IMemoryFlow<string> cache, 
            IOptions<SenderOptions> senderOptions)
        {
            _options = options.Value;
            _cache = cache;
            _clientFactory = clientFactory;
            _senderOptions = senderOptions.Value;
        }


        public async Task<Result<NGeniusPaymentResponse>> CreateOrder(string orderType, string referenceCode, Currencies currency, decimal price, string email, 
            NGeniusBillingAddress billingAddress)
        {
            var order = new OrderRequest
            {
                Action = orderType,
                Amount = new NGeniusAmount
                {
                    CurrencyCode = currency.ToString(),
                    Value = ToNGeniusAmount(price.ToMoneyAmount(currency))
                },
                MerchantOrderReference = referenceCode,
                BillingAddress = billingAddress,
                EmailAddress = email,
                MerchantAttributes = new MerchantAttributes
                {
                    RedirectUrl = new Uri(_senderOptions.BaseUrl, $"/payments/callback?referenceCode={referenceCode}").ToString(),
                    CancelUrl = new Uri(_senderOptions.BaseUrl, "/accommodation/booking").ToString(),
                    CancelText = "Back to Booking Page",
                    SkipConfirmationPage = true
                }
            };
            
            var endpoint = $"transactions/outlets/{_options.Outlets[currency]}/orders";
            var response = await Send(HttpMethod.Post, endpoint, order);

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success(ParseResponseInformation(document))
                : Result.Failure<NGeniusPaymentResponse>(ParseErrorMessage(document));
        }


        public async Task<Result<string>> CaptureMoney(string paymentId, string orderReference, MoneyAmount amount)
        {
            var endpoint = $"transactions/outlets/{_options.Outlets[amount.Currency]}/orders/{orderReference}/payments/{paymentId}/captures";
            var response = await Send(HttpMethod.Post, endpoint, new { Amount = new NGeniusAmount
            {
                CurrencyCode = amount.Currency.ToString(),
                Value = ToNGeniusAmount(amount)
            }});
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success(ParseCaptureId(document))
                : Result.Failure<string>(ParseErrorMessage(document));
        }


        public async Task<Result> VoidMoney(string paymentId, string orderReference, Currencies currency)
        {
            var endpoint = $"transactions/outlets/{_options.Outlets[currency]}/orders/{orderReference}/payments/{paymentId}/cancel";
            var response = await Send(HttpMethod.Put, endpoint);
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(ParseErrorMessage(document));
        }
        
        
        public async Task<Result> RefundMoney(string paymentId, string orderReference, string captureId, MoneyAmount amount)
        {
            var endpoint = $"transactions/outlets/{_options.Outlets[amount.Currency]}/orders/{orderReference}/payments/{paymentId}/captures/{captureId}/refund";
            var response = await Send(HttpMethod.Post, endpoint, new { Amount = new NGeniusAmount
            {
                CurrencyCode = amount.Currency.ToString(),
                Value = ToNGeniusAmount(amount)
            }});
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(ParseErrorMessage(document));
        }


        public async Task<Result<PaymentStatuses>> GetStatus(string orderReference, Currencies currency)
        {
            var endpoint = $"transactions/outlets/{_options.Outlets[currency]}/orders/{orderReference}";
            var response = await Send(HttpMethod.Get, endpoint);
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            return response.IsSuccessStatusCode
                ? ParsePaymentStatus(document)
                : Result.Failure<PaymentStatuses>(ParseErrorMessage(document));
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
            
            return new NGeniusPaymentResponse(paymentId: ParsePaymentId(document),
                orderReference: GetStringValue(rootElement, "reference"),
                merchantOrderReference: GetStringValue(rootElement, "merchantOrderReference"),
                paymentLink: ParsePaymentLink(document));
        }


        private static string ParsePaymentLink(in JsonDocument document)
        {
            return document.RootElement
                .GetProperty("_links")
                .GetProperty("payment")
                .GetProperty("href")
                .GetString();
        }
        
        
        private static string ParsePaymentId(in JsonDocument document)
        {
            return document.RootElement.GetProperty("_embedded")
                .GetProperty("payment")[0]
                .GetProperty("_id")
                .GetString()?
                .Split(':')
                .Last();
        }


        private static PaymentStatuses ParsePaymentStatus(in JsonDocument document)
        {
            var state = document.RootElement.GetProperty("_embedded")
                .GetProperty("payment")[0]
                .GetProperty("state")
                .GetString();
            
            return state switch
            {
                StateTypes.Authorized => PaymentStatuses.Authorized,
                StateTypes.Await3Ds => PaymentStatuses.Secure3d,
                StateTypes.Failed => PaymentStatuses.Failed,
                StateTypes.Captured => PaymentStatuses.Captured,
                StateTypes.Started => PaymentStatuses.Created,
                StateTypes.Reversed => PaymentStatuses.Refunded,
                // StateTypes.PartiallyCaptured not supported
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
        
        
        private static int ToNGeniusAmount(MoneyAmount moneyAmount) 
            => decimal.ToInt32(moneyAmount.Amount * (decimal)Math.Pow(10, moneyAmount.Currency.GetDecimalDigitsCount()));


        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };


        private readonly NGeniusOptions _options;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryFlow<string> _cache;
        private readonly SenderOptions _senderOptions;
    }
}