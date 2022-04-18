using System.Text;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using NATS.Client;

namespace HappyTravel.Edo.Api.Services.Messaging;

public class MessageBus : IMessageBus
{
    public MessageBus(IConnection connection, IJsonSerializer serializer)
    {
        _connection = connection;
        _serializer = serializer;
    }
    
    
    public void Publish<T>(string topic, T message)
    {
        Task.Run(() =>
        {
            _connection.Publish(topic, Encoding.UTF8.GetBytes(_serializer.SerializeObject(message)));
        });
    }


    private readonly IConnection _connection;
    private readonly IJsonSerializer _serializer;
}