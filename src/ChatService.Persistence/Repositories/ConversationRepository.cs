
using ChatService.Contract.Helpers;
using ChatService.Domain.Enums;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class ConversationRepository(IMongoDbContext context) 
    : RepositoryBase<Conversation>(context), IConversationRepository
{
    public async Task<BsonDocument?> GetConversationWithParticipant(ObjectId conversationId, string userId,
        ConversationType type, CancellationToken cancellationToken = default)
    {
        var builder = Builders<Conversation>.Filter;
        var filter = builder.And(
            builder.Eq(conversation => conversation.Id, conversationId),
            builder.Eq(conversation => conversation.ConversationType, type),
            builder.ElemMatch(c => c.Members, m => m.Id == userId));
        
        var participantLookup = new EmptyPipelineDefinition<Participant>()
            .Match(p => p.UserId.Id == userId && p.ConversationId == conversationId && p.IsDeleted == false)
            .Limit(1)
            .AppendStage<Participant, Participant, BsonDocument>(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", nameof(User) },
                { "let", new BsonDocument("userId", "$user_id") },
                {
                    "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument
                        {
                            { "$expr", new BsonDocument("$eq", new BsonArray { "$user_id", "$$userId" }) }
                        })
                    }
                },
                { "as", "user" }
            }))
            .AppendStage<Participant, BsonDocument, BsonDocument>(new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$user" },
                { "preserveNullAndEmptyArrays", true }
            }));

        var projection = new BsonDocument
        {
            { "_id", 1 },
            { "name", 1 },
            { "avatar", 1 },
            { "type", 1 },
            { "status", 1 },
            { "member", 1 }
        };
        
        var document = await DbSet.Aggregate()
            .Match(filter)
            .Lookup<Participant, BsonDocument, IEnumerable<BsonDocument>, BsonDocument>(
                foreignCollection: context.Participants,
                let: new BsonDocument("userId", userId),
                lookupPipeline: participantLookup,
                @as: "member")
            .Unwind("member", new AggregateUnwindOptions<BsonDocument>
            {
                PreserveNullAndEmptyArrays = false
            })
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken);
        
        return document;
    }

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
            // Builders<Conversation>.Update.Set(conversation => conversation.ModifiedDate, CurrentTimeService.GetCurrentTime()),
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
        var builder = Builders<Conversation>.Filter;
        var filter = builder.And(
            builder.Eq(c => c.Id, conversationId),
            builder.Or(
                builder.Eq(c => c.LastMessage, null),
                builder.Lt("last_message.created_date", lastMessage.CreatedDate)));
        var options = new UpdateOptions { IsUpsert = false };
        var updates = new List<UpdateDefinition<Conversation>>
        {
            Builders<Conversation>.Update.Set(conversation => conversation.ModifiedDate, CurrentTimeService.GetCurrentTime()),
            Builders<Conversation>.Update.Set(conversation => conversation.LastMessage, lastMessage)
        };
        
        return await DbSet.UpdateOneAsync(session, filter, Builders<Conversation>.Update.Combine(updates), options, cancellationToken);
    }
}