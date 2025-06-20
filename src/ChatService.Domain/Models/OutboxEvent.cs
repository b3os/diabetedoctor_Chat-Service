using ChatService.Domain.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Domain.Models;

public class OutboxEvent : DomainEntity<ObjectId>
{
     [BsonElement("kafka_topic")]
     public string Topic { get; private init; } = null!;
     [BsonElement("event_type")]
     public string EventType { get; private set; } = null!;
     [BsonElement("message")]
     public string Message { get; private set; } = null!;
     
     [BsonElement("processed_at")]
     public DateTime? ProcessedAt { get; private set; }
     
     [BsonElement("error")]
     public string ErrorMessage { get; private init; } = null!;

     [BsonElement("retry_count")]
     public int RetryCount { get; private set; }
     

     public static OutboxEvent Create(ObjectId id, string topic, string eventTypeName, string message)
     {
          return new OutboxEvent
          {
               Id = id,
               Topic = topic,
               EventType = eventTypeName,
               Message = message,
               ErrorMessage = string.Empty,
               RetryCount = 0,
               CreatedDate = CurrentTimeService.GetCurrentTime(),
               ModifiedDate = CurrentTimeService.GetCurrentTime(),
               ProcessedAt = null,
               IsDeleted = false
          };
     }

     public void IncreaseRetryCount()
     {
          RetryCount++;
     }
}