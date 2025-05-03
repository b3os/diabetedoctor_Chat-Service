using Asp.Versioning.Builder;
using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.Services.Group.Commands;
using ChatService.Contract.Services.User;
using ChatService.Contract.Services.User.Commands;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Presentation.V1;

public static class UserEndpoints
{
    public const string ApiName = "users";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapUserApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup(BaseUrl).HasApiVersion(1);

        // group.MapPost("", CreateUser);
        // group.MapPatch("", UpdateUser);

        return builder;
    }

    // private static async Task UpdateUser(ISender sender)
    // {
    //     var userid = "b93d6316-be4c-4885-a5e0-eae1ea3d1379";
    //     var result = await sender.Send(new UpdateUserCommand {Id = userid, FullName = "Nguyễn Đỗ Chung Quý"});
    // }

    // private static async Task<IResult> CreateUser(ISender sender)
    // {
    //     var result = await sender.Send(new CreateUserCommand ("b93d6316-be4c-4885-a5e0-eae1ea3d1379", "Nguyễn Đỗ Chung Quý", ""));
    //     return Results.Ok(result);
    // } 
}