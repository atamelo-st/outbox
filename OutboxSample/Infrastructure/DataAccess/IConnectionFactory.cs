using System.Data;

namespace OutboxSample.Infrastructure.DataAccess;

public interface IConnectionFactory
{
    IDbConnection GetConnection(string? databaseName = null);
}
