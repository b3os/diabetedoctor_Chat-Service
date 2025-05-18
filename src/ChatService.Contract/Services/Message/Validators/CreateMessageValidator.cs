using ChatService.Contract.Services.Message.Commands;
using FluentValidation;

namespace ChatService.Contract.Services.Message.Validators;

public class CreateMessageValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Nội dung tin nhắn không được để trống");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Loại tin nhắn không hợp lệ.");

        RuleFor(x => x.ReadBy)
            .Must(members => members.All(id => !string.IsNullOrWhiteSpace(id)))
            .WithMessage("Danh sách thành viên không được chứa giá trị rỗng hoặc chỉ có khoảng trắng.")
            .Must(members => members.All(id => Guid.TryParse(id, out _)))
            .WithMessage("Phát hiện thành viên không hợp lệ.");
    }
}