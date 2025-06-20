
using ChatService.Contract.Helpers;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class ConversationRepository(IMongoDbContext context) : RepositoryBase<Conversation>(context), IConversationRepository
{
    public async Task<UpdateResult> UpdateConversationAsync(IClientSessionHandle session, ObjectId conversationId, string? name, Image? avatar, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.Eq(conversation => conversation.Id, conversationId);
        var options = new UpdateOptions { IsUpsert = false };
        var updates = new List<UpdateDefinition<Conversation>>
        {
            Builders<Conversation>.Update.Set(conversation => conversation.ModifiedDate, CurrentTimeService.GetCurrentTime())
        };
        
        if (!string.IsNullOrWhiteSpace(name))
        {
            updates.Add(Builders<Conversation>.Update.Set(conversation => conversation.Name, name));
        }

        if (avatar != null)
        {
            updates.Add(Builders<Conversation>.Update.Set(conversation => conversation.Avatar, avatar));
        }
        
        return await DbSet.UpdateOneAsync(session, filter, Builders<Conversation>.Update.Combine(updates), options, cancellationToken);
    }

    public async Task<UpdateResult> AddMemberToConversationAsync(IClientSessionHandle session, ObjectId conversationId, List<UserId> memberIds, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.Eq(conversation => conversation.Id, conversationId);
        var options = new UpdateOptions { IsUpsert = false };
        var updates = new List<UpdateDefinition<Conversation>>
        {
            Builders<Conversation>.Update.Set(conversation => conversation.ModifiedDate, CurrentTimeService.GetCurrentTime()),
            Builders<Conversation>.Update.AddToSetEach(conversation => conversation.Members, memberIds)
        };
        
        return await DbSet.UpdateOneAsync(session, filter, Builders<Conversation>.Update.Combine(updates), options, cancellationToken);
    }

    public async Task<UpdateResult> RemoveMemberFromConversationAsync(IClientSessionHandle session, ObjectId conversationId, UserId memberId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.Eq(conversation => conversation.Id, conversationId);
        var options = new UpdateOptions { IsUpsert = false };
        var updates = new List<UpdateDefinition<Conversation>>
        {
            Builders<Conversation>.Update.Set(conversation => conversation.ModifiedDate, CurrentTimeService.GetCurrentTime()),
            Builders<Conversation>.Update.Pull(conversation => conversation.Members, memberId)
        };
        
        return await DbSet.UpdateOneAsync(session, filter, Builders<Conversation>.Update.Combine(updates), options, cancellationToken);
    }

    public async Task<UpdateResult> UpdateLastMessageInConversationAsync(IClientSessionHandle session, ObjectId conversationId, Message lastMessage,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.Eq(c => c.Id, conversationId),
            Builders<Conversation>.Filter.Or(
                Builders<Conversation>.Filter.Eq(c => c.LastMessage, null),
                Builders<Conversation>.Filter.Lt("last_message.created_date", lastMessage.CreatedDate)));
        var options = new UpdateOptions { IsUpsert = false };
        var updates = new List<UpdateDefinition<Conversation>>
        {
            Builders<Conversation>.Update.Set(conversation => conversation.ModifiedDate, CurrentTimeService.GetCurrentTime()),
            Builders<Conversation>.Update.Set(conversation => conversation.LastMessage, lastMessage)
        };
        
        return await DbSet.UpdateOneAsync(session, filter, Builders<Conversation>.Update.Combine(updates), options, cancellationToken);
    }
}