using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Infrastructure.Converters
{
    public class DefaultJsonSerializer : IJsonSerializer
    {
        public string SerializeObject<T>(T serializingObject)
        {
            return JsonConvert.SerializeObject(serializingObject);
        }


        public T DeserializeObject<T>(string serializedObject)
        {
            return JsonConvert.DeserializeObject<T>(serializedObject);
        }
    }
}