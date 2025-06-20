using ChatService.Contract.Abstractions.Shared;
using ChatService.Contract.Services;

namespace ChatService.Presentation.V1;

public static class GroupEndpoints
{
    public const string ApiName = "groups";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapGroupApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup(BaseUrl).HasApiVersion(1);

       
        
        // group.MapGet("{groupId}/members", GetMembersOfGroup);
        

        return builder;
    }

    
    
    
    // private static async Task<IResult> GetMembersOfGroup(ISender sender, IClaimsService claimsService, ObjectId groupId)
    // {
    //     var userId = claimsService.GetCurrentUserId;
    //     var result = await sender.Send(new GetGroupMemberByGroupIdQuery() { GroupId = groupId });
    //     return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    // }

    private static IResult HandlerFailure(Result result) =>
        result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException(),
            IValidationResult validationResult =>
                Results.BadRequest(
                    CreateProblemDetails(
                        "Validation Error", StatusCodes.Status400BadRequest,
                        result.Error,
                        validationResult.Errors)),
            _ =>
                Results.BadRequest(
                    CreateProblemDetails(
                        "Bab Request", StatusCodes.Status400BadRequest,
                        result.Error))
        };

    private static ProblemDetails CreateProblemDetails(
        string title,
        int status,
        Error error,
        Error[]? errors = null) =>
        new()
        {
            Title = title,
            Type = error.Code,
            Detail = error.Description.ToString(),
            Status = status,
            Extensions = { { nameof(errors), errors } }
        };
}