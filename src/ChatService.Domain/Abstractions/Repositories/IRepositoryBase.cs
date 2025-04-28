using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatService.Domain.Abstractions.Repositories;

public interface IRepositoryBase<TEntity> where TEntity : DomainEntity<ObjectId>
{
    Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> filter, ProjectionDefinition<TEntity>? definition = default,CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    IQueryable<TEntity> FindAll();
    Task CreateAsync(IClientSessionHandle session, TEntity entity, CancellationToken cancellationToken = default);
    Task CreateManyAsync(IClientSessionHandle session, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<ReplaceOneResult> ReplaceOneAsync(ObjectId id, TEntity entity, CancellationToken cancellationToken = default);
    Task<UpdateResult> SoftDeleteAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<DeleteResult> DeleteOneAsync(ObjectId id, CancellationToken cancellationToken = default);
    Task<DeleteResult> DeleteManyAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
}