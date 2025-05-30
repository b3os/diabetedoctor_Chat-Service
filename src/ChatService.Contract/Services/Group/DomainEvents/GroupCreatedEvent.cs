﻿namespace ChatService.Contract.Services.Group.DomainEvents;

public record GroupCreatedEvent : IDomainEvent
{
    public string GroupId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public IEnumerable<string> Members {get; init;} = null!;
}