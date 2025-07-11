using ChatService.Contract.DTOs.ConversationDtos.Responses;

namespace ChatService.Contract.Services.Conversation.Responses;

public record GetUserConversationsResponse
{
    public PagedList<ConversationResponseDto> Conversations { get; init; } = null!;
}