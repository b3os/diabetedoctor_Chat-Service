using ChatService.Contract.Enums;
using ChatService.Contract.Services.Message.Commands;

namespace ChatService.Contract.Services.Message.Validators;

public class CreateMessageValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageValidator()
    {
        RuleFor(x => x.MessageType)
            .IsInEnum()
            .WithMessage("Loại tin nhắn không hợp lệ.");
        
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung tin nhắn không được để trống")
            .When(x => x.MessageType is MessageTypeEnum.Text);

        RuleFor(x => x.MediaId)
            .NotEmpty().WithMessage("Không tìm thấy file")
            .Must(x => ObjectId.TryParse(x, out _))
            .WithMessage("File đính kèm không hợp lệ.")
            .When(x => x.MessageType is MessageTypeEnum.File);
    }
}