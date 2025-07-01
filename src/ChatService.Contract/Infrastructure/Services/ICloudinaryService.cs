using ChatService.Contract.Enums;
using CloudinaryDotNet.Actions;

namespace ChatService.Contract.Infrastructure.Services;

public interface ICloudinaryService
{
    Task<RawUploadResult> UploadAsync(string id, MediaTypeEnum type, IFormFile formFile, CancellationToken cancellationToken = default);
    Task<DeletionResult?> DeleteAsync(string? publicId);
}