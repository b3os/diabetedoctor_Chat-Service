namespace ChatService.Infrastructure.Options;

public class ConsumerWorkerOptions
{
    public string KafkaGroupId { get; set; } = "consumer-handling";
    public string Topic { get; set; } = null!;
    public IIntegrationEventFactory IntegrationEventFactory { get; set; } = EventBus.IntegrationEventFactory.Instance;
    public string ServiceName { get; set; } = "ConsumerService";
    public Func<IntegrationEvent, bool> AcceptEvent { get; set; } = _ => true;
    public int ServiceRetries { get; set; } = 3;
    public int MaxRetries { get; set; } = 3;
}