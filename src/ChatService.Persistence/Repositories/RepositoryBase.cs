using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatService.Contract.Helpers;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using ChatService.Domain.ValueObjects;
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class RepositoryBase<TEntity> : IRepositoryBase<TEntity>, IDisposable where TEntity : DomainEntity<ObjectId>
{
    protected readonly IMongoCollection<TEntity> DbSet;

    public RepositoryBase(IMongoDbContext context)
    {
        var database = context.Database;
        DbSet = database.GetCollection<TEntity>(typeof(TEntity).Name);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> filter,
        ProjectionDefinition<TEntity> projection = default!, CancellationToken cancellationToken = default)
    {
        return projection switch
        {
            default(ProjectionDefinition<TEntity>) => await DbSet.Find(filter).FirstOrDefaultAsync(cancellationToken),
            _ => await DbSet.Find(filter).Project<TEntity>(projection).FirstOrDefaultAsync(cancellationToken)
        };
    }

    public async Task<TEntity?> FindByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.Find(Builders<TEntity>.Filter.Eq("_id", id)).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await DbSet.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(IClientSessionHandle session, TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.InsertOneAsync(session: session, entity, cancellationToken: cancellationToken);
    }

    public async Task CreateManyAsync(IClientSessionHandle session, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.InsertManyAsync(session: session, entities, cancellationToken: cancellationToken);
    }

    public async Task<UpdateResult> UpdateOneAsync(IClientSessionHandle session, ObjectId id, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        var finalUpdate = Builders<TEntity>.Update.Combine(update, Builders<TEntity>.Update.Set(x => x.ModifiedDate, DateTime.UtcNow));
        return await DbSet.UpdateOneAsync(session, filter, finalUpdate, new UpdateOptions { IsUpsert = false }, cancellationToken);
    }

    public async Task<UpdateResult> UpdateManyAsync(IClientSessionHandle session, FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition, CancellationToken cancellationToken = default)
    {
        return await DbSet.UpdateManyAsync(session: session, filterDefinition, updateDefinition, new UpdateOptions { IsUpsert = false }, cancellationToken);
    }

    public async Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, TEntity entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id);
        return await DbSet.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }
    
    public async Task<DeleteResult> DeleteOneAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);
        return await DbSet.DeleteOneAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<DeleteResult> DeleteManyAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var deleteIds = new List<ObjectId>();
        foreach (var id in ids)
        {
            if (ObjectId.TryParse(id, out var objectId))
            {
                deleteIds.Add(objectId);
            }
        }

        if (deleteIds.Count == 0)
            return DeleteResult.Unacknowledged.Instance;

        var filter = Builders<TEntity>.Filter.In("_id", deleteIds);
        return await DbSet.DeleteManyAsync(filter, cancellationToken: cancellationToken);
    }
}