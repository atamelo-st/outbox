using System.Data.Common;

namespace OutboxSample.Infrastructure.DataAccess;

public interface IConnectionFactory
{
    DbConnection GetConnection(string? databaseName = null);
}
