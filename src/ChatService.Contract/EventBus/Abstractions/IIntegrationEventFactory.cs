using ChatService.Contract.EventBus.Abstractions.Message;

namespace ChatService.Contract.EventBus.Abstractions;

public interface IIntegrationEventFactory
{
    IntegrationEvent? CreateEvent(string typeName, string value);
}