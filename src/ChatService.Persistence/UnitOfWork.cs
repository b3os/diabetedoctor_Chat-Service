namespace ChatService.Persistence;

public class UnitOfWork(IMongoDbContext context) : IUnitOfWork
{
    private IClientSessionHandle? _clientSession;
    private readonly MongoClient _client = context.Client;

    public IClientSessionHandle ClientSession => _clientSession ?? throw new InvalidOperationException("Transaction has not been started.");

    public async Task StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_clientSession is not null)
        {
            return;
        }

        _clientSession = await _client.StartSessionAsync(cancellationToken: cancellationToken);
        _clientSession.StartTransaction();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_clientSession is null)
        {
            throw new InvalidOperationException("A transaction has not been started.");
        }
        
        try
        {
            await _clientSession.CommitTransactionAsync(cancellationToken);
            _clientSession.Dispose();
            _clientSession = null;
        }
        catch (Exception)
        {
            if (_clientSession is not null)
            {
                await _clientSession.AbortTransactionAsync(cancellationToken);
            }
            throw;
        }
    }

    public async Task AbortTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_clientSession is not null)
            {
                await _clientSession.AbortTransactionAsync(cancellationToken);
            }
        }
        finally
        {
            _clientSession?.Dispose();
            _clientSession = null;
        }
    }
}