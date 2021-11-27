using System.Data;

namespace OutboxSample.Infrastructure;

public class RegularConnectionFactory : IConnectionFactory
{
    public IDbConnection GetConnection()
    {
        throw new NotImplementedException();
    }
}
