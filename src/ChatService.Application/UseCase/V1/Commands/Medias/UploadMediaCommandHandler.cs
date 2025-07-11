using ChatService.Contract.Enums;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services.Media.Commands;
using ChatService.Contract.Services.Media.Responses;
using CloudinaryDotNet.Actions;

namespace ChatService.Application.UseCase.V1.Commands.Medias;

public sealed class UploadMediaCommandHandler(
    IUnitOfWork unitOfWork,
    IMediaRepository mediaRepository,
    ICloudinaryService cloudinary)
    : ICommandHandler<UploadMediaCommand, Response<UploadMediaResponse>>
{
    public async Task<Result<Response<UploadMediaResponse>>> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        var mediaList = new List<Media>();
        try
        {
            foreach (var file in request.Files)
            {
                var type = FileExtension.DetectFromExtension(file.FileName);
                var id = ObjectId.GenerateNewId();
                var uploadResult = await cloudinary.UploadAsync(id.ToString(), type, file, cancellationToken);
                if (uploadResult.Error != null)
                {
                    throw new Exception($"Lỗi khi upload file: {uploadResult.Error.Message}");
                }
                mediaList.Add(MapToMedia(id, uploadResult, type, request.UploaderId!));
            }

            if (mediaList.Count > 0)
            {
                await unitOfWork.StartTransactionAsync(cancellationToken);
                await mediaRepository.CreateManyAsync(unitOfWork.ClientSession, mediaList, cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);
            }
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            foreach (var media in mediaList)
            {
                await cloudinary.DeleteAsync(media.PublicId);
            }
            throw;
        }

        var mediaIds = mediaList.Select(x => x.Id.ToString()).ToList();
        return Result.Success(new Response<UploadMediaResponse>(
            MediaMessage.UploadMediaSuccessfully.GetMessage().Code,
            MediaMessage.UploadMediaSuccessfully.GetMessage().Message,
            new UploadMediaResponse{MediaIds = mediaIds}));
    }

    private Media MapToMedia(ObjectId id, RawUploadResult uploadResult, MediaTypeEnum type, string uploadBy)
    {
        var userId = UserId.Of(uploadBy);
        var mediaType = type.ToEnum<MediaTypeEnum, MediaType>();
        return Media.Create(
            id: id,
            publicId: uploadResult.FullyQualifiedPublicId,
            publicUrl: uploadResult.SecureUrl.AbsoluteUri,
            fileName: uploadResult.OriginalFilename,
            mediaType,
            userId);
    }
}