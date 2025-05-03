using ChatService.Contract.Services.User;
using ChatService.Contract.Services.User.Commands;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObjects;
using MongoDB.Driver;

namespace ChatService.Application.UseCase.V1.Commands.User;

public class UpdateUserCommandHandler (IUserRepository userRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var projection = Builders<Domain.Models.User>.Projection
            .Include(x => x.Fullname)
            .Include(x => x.Avatar);
        var user = await userRepository.FindSingleAsync(x => x.UserId.Id.Equals(request.Id), projection, cancellationToken);

        if (user is null)
        {
            return Result.Failure(new Error("ABC",$"User with id {request.Id} does not exist."));
        }

        user.Modify(request.FullName, string.IsNullOrWhiteSpace(request.Avatar) ? null : Image.Of(request.Avatar));

        if (user.Changes.Count == 0)
        {
            return Result.Success();
        }
        
        await unitOfWork.StartTransactionAsync(cancellationToken);
        try
        {
            await userRepository.UpdateOneAsync(unitOfWork.ClientSession, user, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.AbortTransactionAsync(cancellationToken);
            throw;
        }
        
        return Result.Success();
    }
}