﻿
namespace ChatService.Domain.Abstractions.Repositories;

public interface IUserRepository : IRepositoryBase<User>
{
    IMongoCollection<User> GetAllUsers();
}