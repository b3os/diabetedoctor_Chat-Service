using ChatService.Contract.Services.Conversation.Commands;
using FluentValidation;

namespace ChatService.Contract.Services.Conversation.Validators;

public class CreateConversationValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên nhóm không được để trống");
        
        RuleFor(x => x.Members)
            .Must(members => members.All(id => !string.IsNullOrWhiteSpace(id)))
            .WithMessage("Danh sách thành viên không được chứa giá trị rỗng hoặc chỉ có khoảng trắng.")
            .Must(members => members.All(id => Guid.TryParse(id, out _)))
            .WithMessage("Phát hiện thành viên không hợp lệ.")
            .When(x => x.Members is { Count: > 0 });
    }
}