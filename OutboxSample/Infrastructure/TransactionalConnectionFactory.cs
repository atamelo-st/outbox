using System.Data;

namespace OutboxSample.Infrastructure;

public class TransactionalConnectionFactory : IConnectionFactory
{
    public IDbConnection GetConnection()
    {
        // TODO: create wrapper around regalar IDbConnection to handle connection
        // (spanning multiple sql commands) as well as transaction creation

        throw new NotImplementedException();
    }
}
