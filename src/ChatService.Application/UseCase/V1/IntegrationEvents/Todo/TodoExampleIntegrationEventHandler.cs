using System;
using System.Threading;
using System.Threading.Tasks;
using ChatService.Contract.EventBus.Abstractions.Message;
using ChatService.Contract.EventBus.Events.TodoIntegrationEvents;

namespace ChatService.Application.UseCase.V1.IntegrationEvents.Todo;

public class TodoExampleIntegrationEventHandler : IIntegrationEventHandler<TodoExampleIntegrationEvent>
{
    public async Task Handle(TodoExampleIntegrationEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine(notification.Example);
        await Task.CompletedTask.WaitAsync(cancellationToken);
    }
}