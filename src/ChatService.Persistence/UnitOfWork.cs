using ChatService.Domain.Abstractions;

namespace ChatService.Persistence;

public class UnitOfWork(MongoDbContext context) : IUnitOfWork
{
    private IClientSessionHandle? _clientSession;
    private readonly MongoClient _client = context.Client;

    public IClientSessionHandle ClientSession => _clientSession ?? throw new InvalidOperationException("Transaction has not been started.");

    public async Task StartTransactionAsync()
    {
        if (_clientSession is not null)
        {
            return;
        }

        _clientSession = await _client.StartSessionAsync();
        _clientSession.StartTransaction();
    }

    public async Task CommitTransactionAsync()
    {
        if (_clientSession is null)
        {
            throw new InvalidOperationException("A transaction has not been started.");
        }
        
        try
        {
            await _clientSession.CommitTransactionAsync();
            _clientSession.Dispose();
            _clientSession = null;
        }
        catch (Exception)
        {
            if (_clientSession is not null)
            {
                await _clientSession.AbortTransactionAsync();
            }
            throw;
        }
    }

    public async Task AbortTransactionAsync()
    {
        try
        {
            if (_clientSession is not null)
            {
                await _clientSession.AbortTransactionAsync();
            }
        }
        finally
        {
            _clientSession?.Dispose();
            _clientSession = null;
        }
    }
}