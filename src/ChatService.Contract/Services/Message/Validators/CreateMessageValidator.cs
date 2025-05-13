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
    }
}