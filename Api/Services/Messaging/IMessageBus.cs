namespace HappyTravel.Edo.Api.Services.Messaging;

public interface IMessageBus
{
    void Publish<T>(string topicName, T message);
    void Publish(string topicName);
}