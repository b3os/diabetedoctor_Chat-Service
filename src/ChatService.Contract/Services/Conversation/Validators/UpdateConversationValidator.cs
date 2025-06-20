using ChatService.Contract.Services.Conversation.Commands;
using FluentValidation;

namespace ChatService.Contract.Services.Conversation.Validators;

public class UpdateConversationValidator : AbstractValidator<UpdateConversationCommand>
{
    public UpdateConversationValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.Avatar))
            .WithMessage("Bạn phải cập nhật tên nhóm hoặc ảnh đại diện.");
    }
}