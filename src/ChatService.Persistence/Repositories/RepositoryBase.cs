using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatService.Domain.Abstractions;
using ChatService.Domain.Abstractions.Repositories;
using MongoDB.Bson;

namespace ChatService.Persistence.Repositories;

public class RepositoryBase<TEntity> : IRepositoryBase<TEntity>, IDisposable where TEntity : DomainEntity<ObjectId>
{
    private readonly IMongoCollection<TEntity> _dbSet;

    public RepositoryBase(MongoDbContext context)
    {
        var database = context.Database;
        _dbSet = database.GetCollection<TEntity>(typeof(TEntity).Name);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> filter, ProjectionDefinition<TEntity>? projection = default,CancellationToken cancellationToken = default)
    {
        return await _dbSet.Find(filter).Project<TEntity>(projection).SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FindByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Find(Builders<TEntity>.Filter.Eq("_id", id)).SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Find(filter).ToListAsync(cancellationToken: cancellationToken);
    }

    public IQueryable<TEntity> FindAll()
    {
        return  _dbSet.AsQueryable();
    }

    public async Task CreateAsync(IClientSessionHandle session, TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.InsertOneAsync(session: session, entity, cancellationToken: cancellationToken);
    }

    public async Task CreateManyAsync(IClientSessionHandle session, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.InsertManyAsync(session: session, entities, cancellationToken: cancellationToken);
    }

    public async Task<ReplaceOneResult> ReplaceOneAsync(ObjectId id, TEntity entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);
        return await _dbSet.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }

    public async Task<UpdateResult> SoftDeleteAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);
        var update = Builders<TEntity>.Update.Set(x => x.IsDeleted, true);
        return await _dbSet.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task<DeleteResult> DeleteOneAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("_id", id);
        return await _dbSet.DeleteOneAsync(filter, cancellationToken: cancellationToken);
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
        return await _dbSet.DeleteManyAsync(filter, cancellationToken: cancellationToken);
    }
}