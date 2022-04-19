namespace HappyTravel.Edo.Api.Services.Messaging;

public interface IMessageBus
{
    void Publish<T>(string topic, T message);
    void Publish(string topic);
}