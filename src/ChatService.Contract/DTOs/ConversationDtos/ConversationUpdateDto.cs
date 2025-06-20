namespace ChatService.Contract.DTOs.ConversationDtos;

public record ConversationUpdateDto(string? Name , string? Avatar, int Version);