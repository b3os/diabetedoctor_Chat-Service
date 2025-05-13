using ChatService.Contract.Common.Messages;
using ChatService.Contract.Services.Group.Commands;
using FluentValidation;

namespace ChatService.Contract.Services.Group.Validators;

public class CreateGroupValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên nhóm không được để trống");
        
        RuleFor(x => x.Members)
            .Must(members => members.All(id => !string.IsNullOrWhiteSpace(id)))
            .WithMessage("Danh sách thành viên không được chứa giá trị rỗng hoặc chỉ có khoảng trắng.")
            .Must(members => members.All(id => Guid.TryParse(id, out _)))
            .WithMessage("Phát hiện thành viên không hợp lệ.");
        
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Chủ phòng không được để trống.")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Chủ phòng không hợp lệ.");
    }
}