using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Domain.Abstractions;
using ChatService.Domain.ValueObjects;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.Group;

public class PromoteGroupMemberCommandHandler(
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<PromoteGroupMemberCommand>
{
    public async Task<Result> Handle(PromoteGroupMemberCommand request, CancellationToken cancellationToken)
    {
        await EnsureUserExistsAsync(request.MemberId, cancellationToken);
        
        await EnsureGroupOwnerAccessAsync(request.GroupId, request.MemberId, cancellationToken);

        var member = await groupRepository.GetGroupMemberInfoAsync(request.GroupId, request.MemberId, cancellationToken);

        if (member is null)
        {
            throw new GroupExceptions.GroupMemberNotExistsException();
        }

        if (member["role"] == GroupRoleEnum.Owner)
        {
            throw new GroupExceptions.CannotDemoteOwnerException();
        }

        var isPromoted = member["role"] == GroupRoleEnum.Member;
        var builder = Builders<Domain.Models.Group>.Update;

        var arrayFilters = new List<ArrayFilterDefinition>()
        {
            new BsonDocumentArrayFilterDefinition<BsonDocument>(
                new BsonDocument("member.user_id._id", request.MemberId))
        };

        var updateOptions = new UpdateOptions<Domain.Models.Group> { ArrayFilters = arrayFilters };

        var update = isPromoted
            ? builder.Set("members.$[member].role", GroupRoleEnum.Admin)
            : builder.Set("members.$[member].role", GroupRoleEnum.Member);

        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await groupRepository.UpdateOneAsync(unitOfWork.ClientSession, request.GroupId, update, updateOptions,
                cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }

        var message = isPromoted
            ? GroupMessage.PromoteMemberToAdminSuccessfully.GetMessage()
            : GroupMessage.DemoteAdminToMemberSuccessfully.GetMessage();

        return Result.Success(new Response(message.Code, message.Message));
    }
    
    private async Task EnsureUserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        var exists = await userRepository.ExistsAsync(u => u.UserId.Id == userId, cancellationToken);
        if (!exists)
            throw new UserExceptions.UserNotFoundException();
    }
    
    private async Task EnsureGroupOwnerAccessAsync(ObjectId groupId, string ownerId, CancellationToken cancellationToken)
    {
        var groupExist = await groupRepository.ExistsAsync(
            group => group.Id == groupId && group.Members.Any(m => m.UserId.Id == ownerId && m.Role == GroupRoleEnum.Owner),
            cancellationToken);

        if (!groupExist)
            throw new GroupExceptions.GroupAccessDeniedException();
    }
}