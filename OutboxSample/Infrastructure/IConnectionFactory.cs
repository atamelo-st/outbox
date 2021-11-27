using System.Data;

namespace OutboxSample.Infrastructure;

public interface IConnectionFactory
{
    IDbConnection GetConnection();
}
