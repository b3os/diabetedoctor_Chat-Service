using ChatService.Contract.Services.User.Commands;

namespace ChatService.Presentation.V1;

public static class UserEndpoints
{
    public const string ApiName = "users";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapUserApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup(BaseUrl).HasApiVersion(1);

        group.MapPost("", CreateUser);
        // group.MapPatch("", UpdateUser);

        return builder;
    }

    // private static async Task UpdateUser(ISender sender)
    // {
    //     var userid = "b93d6316-be4c-4885-a5e0-eae1ea3d1379";
    //     var result = await sender.Send(new UpdateUserCommand {Id = userid, FullName = "Nguyễn Đỗ Chung Quý"});
    // }

    private static async Task<IResult> CreateUser(ISender sender)
    {
        var result = await sender.Send(new CreateUserCommand
        {
            Id = "ab88428d-2c9b-439f-b497-6c39cb77f80f",
            FullName = "Nguyễn Đỗ Chung Quý",
            Avatar = ""
        });
        return Results.Ok(result);
    } 
}