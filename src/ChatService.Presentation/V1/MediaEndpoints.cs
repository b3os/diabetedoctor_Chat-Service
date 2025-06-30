using ChatService.Contract.Services.Media.Commands;

namespace ChatService.Presentation.V1;

public static class MediaEndpoints
{
    public const string ApiName = "media";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";
    
    public static IVersionedEndpointRouteBuilder MapMediaApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var media = builder.MapGroup(BaseUrl).HasApiVersion(1).DisableAntiforgery();
        media.MapPost("upload", UploadMedia).RequireAuthorization();
        return builder;
    }
    
    private static async Task<IResult> UploadMedia(ISender sender, IClaimsService claimsService,
        [FromForm] UploadMediaCommand dto)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(dto with { UploaderId = userId });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
}