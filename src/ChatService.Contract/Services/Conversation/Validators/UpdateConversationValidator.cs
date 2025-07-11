using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Contract.Services.Conversation.Validators;

public class UpdateConversationValidator : AbstractValidator<UpdateGroupConversationCommand>
{
    public UpdateConversationValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Name is not null || x.AvatarId is not null)
            .WithMessage("Bạn phải cập nhật tên nhóm hoặc ảnh đại diện.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống")
            .When(x => x.Name is not null);
        
        RuleFor(x => x.AvatarId)
            .NotEmpty().WithMessage("Không tìm thấy ảnh")
            .Must(x => ObjectId.TryParse(x, out _))
            .WithMessage("File ảnh không hợp lệ.")
            .When(x => x.AvatarId is not null);
    }
}