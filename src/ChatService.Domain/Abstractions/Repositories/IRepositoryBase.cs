﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatService.Domain.Abstractions.Repositories;

public interface IRepositoryBase<TEntity> where TEntity : DomainEntity<ObjectId>
{
    Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> filter,
        ProjectionDefinition<TEntity> definition = null!, CancellationToken cancellationToken = default);

    Task<TEntity?> FindByIdAsync(ObjectId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> FindListAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    Task CreateAsync(IClientSessionHandle session, TEntity entity, CancellationToken cancellationToken = default);

    Task CreateManyAsync(IClientSessionHandle session, IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    Task<UpdateResult> UpdateOneAsync(IClientSessionHandle session, ObjectId id, UpdateDefinition<TEntity> update,
        UpdateOptions<TEntity> options = null!, CancellationToken cancellationToken = default);

    Task<UpdateResult> UpdateManyAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter,
        UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default);

    Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, TEntity entity,
        CancellationToken cancellationToken = default);

    Task<DeleteResult> DeleteOneAsync(IClientSessionHandle session, ObjectId id, CancellationToken cancellationToken = default);
    Task<DeleteResult> DeleteManyAsync(IClientSessionHandle session, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default);
}