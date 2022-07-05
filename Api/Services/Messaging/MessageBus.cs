using System;
using System.Text.Json;
using System.Threading.Tasks;
using NATS.Client;

namespace HappyTravel.Edo.Api.Services.Messaging;

public class MessageBus : IMessageBus
{
    public MessageBus(IConnection connection)
    {
        _connection = connection;
    }
    
    
    public void Publish<T>(string topicName, T message)
    {
        Task.Run(() =>
        {
            _connection.Publish(topicName, JsonSerializer.SerializeToUtf8Bytes(message));
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
}