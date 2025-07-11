using ChatService.Contract.Services.Conversation.Commands.GroupConversation;

namespace ChatService.Contract.Services.Conversation.Validators;

public class AddMemberToGroupValidator : AbstractValidator<AddMembersToGroupCommand>
{
    public AddMemberToGroupValidator()
    {
        RuleFor(x => x.UserIds)
            .Must(members => members is { Count: > 0 })
            .Must(members => members.All(id => !string.IsNullOrWhiteSpace(id)))
            .WithMessage("Danh sách thành viên không được chứa giá trị rỗng hoặc chỉ có khoảng trắng.")
            .Must(members => members.All(id => Guid.TryParse(id, out _)))
            .WithMessage("Phát hiện thành viên không hợp lệ.");
    }
}