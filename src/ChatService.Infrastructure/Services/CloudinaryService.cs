using ChatService.Contract.Enums;
using ChatService.Contract.Infrastructure.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace ChatService.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly IOptions<CloudinarySettings> _cloudinarySettings;

    public CloudinaryService(IOptions<CloudinarySettings> settings)
    {   
        _cloudinarySettings = settings;
        var account = new Account
        {
            Cloud = settings.Value.CloudName,
            ApiKey = settings.Value.ApiKey,
            ApiSecret = settings.Value.ApiSecret
        };
        _cloudinary = new Cloudinary(account);
    }

    public async Task<RawUploadResult> UploadAsync(string id, MediaTypeEnum type, IFormFile formFile, CancellationToken cancellationToken = default)
    {
        await using var stream = formFile.OpenReadStream();
        var file = new FileDescription(Path.GetFileName(formFile.FileName), stream);
        var publicId = _cloudinarySettings.Value.Folder + "/" + type.ToString().ToLower() + "/" + id;
        return type switch
        {
            MediaTypeEnum.Image => await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File = file,
                PublicId = publicId,
                UseFilename = true,
                Overwrite = true,
            }),

            MediaTypeEnum.Video => await _cloudinary.UploadAsync(new VideoUploadParams
            {
                File = file,
                PublicId = publicId,
                UseFilename = true,
                Overwrite = true,
            }),

            _ => await _cloudinary.UploadAsync(new RawUploadParams
            {
                File = file,
                PublicId = publicId,
                UseFilename = true,
                Overwrite = true,
            })
        };
    }

    public async Task<DeletionResult?> DeleteAsync(string? publicId)
    {
        return publicId is not null 
            ? await _cloudinary.DestroyAsync(new DeletionParams(publicId)) 
            : null;
    }


    
}