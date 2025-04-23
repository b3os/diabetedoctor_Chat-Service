namespace ChatService.Domain.Abstractions;

public interface IUnitOfWork
{
    IClientSessionHandle ClientSession { get; }
    
    Task StartTransactionAsync();
    Task CommitTransactionAsync();
    Task AbortTransactionAsync();
}