using System.ComponentModel.DataAnnotations;
using Asp.Versioning.Builder;
using ChatService.Contract.Abstractions.Shared;
using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.Infrastructure.Services;
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

        group.MapPost("", CreateGroup).RequireAuthorization().WithSummary("Create a new group");
        group.MapPost("{groupId}/members", AddMemberToGroup).RequireAuthorization().WithSummary("Add new members to the group");

        group.MapPatch("{groupId}", UpdateGroup).RequireAuthorization().WithSummary("Update a group");
        group.MapPatch("{groupId}/members/{userId}", PromoteGroupMember).RequireAuthorization().WithSummary("Promote a group member to admin");

        group.MapGet("", GetUserGroup).RequireAuthorization().WithSummary("Get groups of a user");
        // group.MapGet("{groupId}/members", GetMembersOfGroup);
        
        group.MapDelete("{groupId}/members/{memberId}", RemoveMemberFromGroup).RequireAuthorization().WithSummary("Remove a group member");

        return builder;
    }

    private static async Task<IResult> CreateGroup(ISender sender, IClaimsService claimsService,
        [FromBody] CreateGroupCommand command)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(command with { OwnerId = ownerId });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> UpdateGroup(ISender sender, IClaimsService claimsService, ObjectId groupId,
        [FromBody] GroupUpdateDto dto)
    {
        var adminId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new UpdateGroupCommand() { AdminId = adminId, GroupId = groupId, Avatar = dto.Avatar , Name = dto.Name });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> PromoteGroupMember(ISender sender, IClaimsService claimsService, ObjectId groupId,
        string userId)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new PromoteGroupMemberCommand
            { OwnerId = ownerId, GroupId = groupId, MemberId = userId });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetUserGroup(ISender sender, IClaimsService claimsService,
        [AsParameters] QueryFilter filter)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new GetUserGroupByUserIdQuery() { UserId = userId, Filter = filter });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> AddMemberToGroup(ISender sender, IClaimsService claimsService, ObjectId groupId,
        [FromBody] GroupAddMemberDto dto)
    {
        var adminId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new AddMemberToGroupCommand() { AdminId = adminId, GroupId = groupId, UserIds = dto.UserIds });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> RemoveMemberFromGroup(ISender sender, IClaimsService claimsService, ObjectId groupId,
        string memberId)
    {
        var adminId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new RemoveMemberFromGroupCommand() { AdminId = adminId, GroupId = groupId, MemberId = memberId });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetMembersOfGroup(ISender sender, IClaimsService claimsService, ObjectId groupId)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new GetGroupMemberByGroupIdQuery() { GroupId = groupId });
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