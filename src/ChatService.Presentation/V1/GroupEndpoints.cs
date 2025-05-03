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

        group.MapPost("", CreateGroup).WithSummary("Create a new group");
        group.MapPost("{groupId}/members", AddMemberToGroup).WithSummary("Add new members to the group");

        group.MapPatch("{groupId}", UpdateGroup).WithSummary("Update a group");
        group.MapPatch("{groupId}/members/{userId}", PromoteGroupMember).WithSummary("Promote a group member to admin");

        group.MapGet("", GetUserGroup).WithSummary("Get groups of a user");


        return builder;
    }

    private static async Task<IResult> CreateGroup(ISender sender, IClaimsService claimsService,
        [FromBody] CreateGroupCommand command)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(command with { OwnerId = ownerId });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> UpdateGroup(ISender sender, IClaimsService claimsService, string groupId,
        [FromBody] UpdateGroupCommand command)
    {
        var adminId = claimsService.GetCurrentUserId;
        var result = await sender.Send(command with { AdminId = adminId, GroupId = groupId });
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> PromoteGroupMember(ISender sender, IClaimsService claimsService, string groupId,
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

    private static async Task<IResult> AddMemberToGroup(ISender sender, IClaimsService claimsService, string groupId,
        [FromBody] AddMemberToGroupCommand command)
    {
        var adminId = claimsService.GetCurrentUserId;
        var result = await sender.Send(command with { AdminId = adminId, GroupId = groupId });
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