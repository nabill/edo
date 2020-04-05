using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HappyTravel.Edo.Api.Services.Payments.Payfort
{
    internal static class PayfortSerializationSettings
    {
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore
        };

        public static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings);
    }
}