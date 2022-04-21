using System;
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
    
    
    public void Publish<T>(string topicName, T message)
    {
        Task.Run(() =>
        {
            _connection.Publish(topicName, Encoding.UTF8.GetBytes(_serializer.SerializeObject(message)));
        });
    }


    public void Publish(string topicName)
    {
        Task.Run(() =>
        {
            _connection.Publish(topicName, Array.Empty<byte>());
        });
    }


    private readonly IConnection _connection;
    private readonly IJsonSerializer _serializer;
}