using ChatService.Contract.DTOs.ConversationDtos;

namespace ChatService.Contract.Services.Conversation.Responses;

public record GetUserConversationsResponse
{
    public PagedList<ConversationResponseDto> Conversations { get; init; } = null!;
}