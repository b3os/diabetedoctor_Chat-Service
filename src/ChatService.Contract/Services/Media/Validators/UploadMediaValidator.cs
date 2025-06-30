using ChatService.Contract.Services.Media.Commands;

namespace ChatService.Contract.Services.Media.Validators;

public class UploadMediaValidator : AbstractValidator<UploadMediaCommand>
{
    public UploadMediaValidator()
    {
        RuleFor(x => x.Files)
            .NotEmpty()
            .WithMessage("Phải upload ít nhất 1 file")
            .Must(files => files.Count < 5)
            .WithMessage("Chỉ nhận 5 file tối đa")
            .Must(files => files.All(file => file is { Length: > 0 }))
            .WithMessage("Phát hiện có file rỗng");

        RuleForEach(x => x.Files)
            .Must(file => FileExtension.HasImageExtension(file.FileName))
            .WithMessage("File phải là ảnh.")
            .Must(file => file.Length >= 1024)
            .WithMessage("File phải có dung lượng lớn hơn 1KB.")
            .Must(file => file.Length < 100 * 1024 * 1024)
            .WithMessage("File phải có dung lượng nhỏ hơn 100MB.");
    }
}