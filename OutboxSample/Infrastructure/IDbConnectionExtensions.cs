using System.Data;

namespace OutboxSample.Infrastructure;

public static class IDbConnectionExtensions
{
    public static TDbCommand CreateCommand<TDbCommand>(this IDbConnection connection) where TDbCommand : IDbCommand 
        => (TDbCommand)connection.CreateCommand();
}