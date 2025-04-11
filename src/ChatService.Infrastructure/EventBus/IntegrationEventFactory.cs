using System;
using System.Linq;

namespace ChatService.Infrastructure.EventBus;

public class IntegrationEventFactory : IIntegrationEventFactory
{
    public IntegrationEvent? CreateEvent(string typeName, string value)
    {
        var t = GetEventType(typeName) ?? throw new ArgumentException($"Type {typeName} not found");

        return JsonSerializer.Deserialize(value, t) as IntegrationEvent;
    }
    
    private static Type? GetEventType(string type)
    {
        var t = Type.GetType(type);

        return t ?? AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(aType => aType.FullName == type);
    }

    public static readonly IntegrationEventFactory Instance = new();
}

public class IntegrationEventFactory<TEvent> : IIntegrationEventFactory
{
    private static readonly Assembly IntegrationEventAssembly = typeof(TEvent).Assembly;

    public IntegrationEvent? CreateEvent(string typeName, string value)
    {
        var t = GetEventType(typeName) ?? throw new ArgumentException($"Type {typeName} not found");

        return JsonSerializer.Deserialize(value, t) as IntegrationEvent;
    }

    private static Type? GetEventType(string typeName)
    {
        var t = IntegrationEventAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == typeName);

        return t ?? AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(aType => aType.Name == typeName);
    }

    public static readonly IntegrationEventFactory<TEvent> Instance = new();
}