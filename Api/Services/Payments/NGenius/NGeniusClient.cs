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
            var token = await GetAccessToken();
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(order, SerializerOptions), null, "application/vnd.ni-payment.v2+json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var client = _clientFactory.CreateClient(HttpClientNames.NGenius);
            var response = await client.SendAsync(request);

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            if (!response.IsSuccessStatusCode)
            {
                var messages = document.RootElement.GetProperty("errors")
                    .EnumerateArray()
                    .Select(e => e.GetProperty("localizedMessage").GetString())
                    .ToList();
                return Result.Failure<NGeniusPaymentResponse>(string.Join(';', messages));
            }

            var paymentId = document.RootElement
                .GetProperty("_id")
                .GetString()?
                .Split(":")
                .Last();

            var state = document.RootElement
                .GetProperty("state")
                .GetString();

            var merchantOrderReference = document.RootElement
                .GetProperty("orderReference")
                .GetString();

            var paymentMethod = document.RootElement.GetProperty("paymentMethod");
            var expiry = paymentMethod.GetProperty("expiry").GetString();
            var cardholderName = paymentMethod.GetProperty("cardholderName").GetString();
            var pan = paymentMethod.GetProperty("pan").GetString();
            var cvv = paymentMethod.GetProperty("cvv").GetString();
            var name = paymentMethod.GetProperty("name").GetString();
            var paymentInformation = new ResponsePaymentInformation(pan, expiry, cvv, cardholderName, name);

            var status = state switch
            {
                StateTypes.Authorized => CreditCardPaymentStatuses.Success,
                StateTypes.Await3Ds => CreditCardPaymentStatuses.Secure3d,
                StateTypes.Failed => CreditCardPaymentStatuses.Failed,
                _ => throw new NotSupportedException($"Payment status `{state}` not supported")
            };

            Secure3dOptions? secure3dOptions = null;
            if (status == CreditCardPaymentStatuses.Secure3d)
            {
                var element = document.RootElement.GetProperty("3ds");
                var acs = element.GetProperty("acsUrl").GetString();
                var acsPaReq = element.GetProperty("acsPaReq").GetString();
                var acsMd = element.GetProperty("acsMd").GetString();
                secure3dOptions = new Secure3dOptions(acs, acsPaReq, acsMd);
            }

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