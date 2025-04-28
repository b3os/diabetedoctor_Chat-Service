using System.ComponentModel.DataAnnotations;
using Asp.Versioning.Builder;
using ChatService.Contract.Abstractions.Shared;
using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.Services;
using ChatService.Contract.Services.Group;
using ChatService.Contract.Services.Group.Commands;
using ChatService.Contract.Services.Group.Queries;
using ChatService.Contract.Services.Message.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace ChatService.Presentation.V1;

public static class GroupEndpoints
{
    public const string ApiName = "groups";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapGroupApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup(BaseUrl).HasApiVersion(1);

        group.MapPost("", CreateGroup);
        group.MapPatch("{groupId}", UpdateGroup);
        group.MapPatch("{groupId}/members/{userId}", PromoteGroupMember);
        group.MapGet("", GetUserGroup);

        return builder;
    }

    private static async Task<IResult> CreateGroup(ISender sender, [FromBody] GroupCreateDto dto)
    {
        var result = await sender.Send(new CreateGroupCommand {Group = dto});
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> UpdateGroup(ISender sender, [Required] string groupId, [FromBody] GroupUpdateDto dto)
    {
        var result = await sender.Send(new UpdateGroupCommand {GroupId = groupId, GroupUpdateDto = dto});
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> PromoteGroupMember(ISender sender, [Required] string groupId, [Required] string userId, [FromBody] GroupUpdateDto dto)
    {
        var result = await sender.Send(new PromoteGroupMemberCommand {GroupId = groupId, MemberId = userId});
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetUserGroup(ISender sender, [AsParameters] QueryFilter filter)
    {
        var result = await sender.Send(new GetUserGroupByUserIdQuery() {Filter = filter});
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    } 
    
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
            Detail = error.Message,
            Status = status,
            Extensions = { { nameof(errors), errors } }
        };
}