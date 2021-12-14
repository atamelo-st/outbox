using System.Data;
using System.Data.Common;

namespace OutboxSample.Infrastructure.DataAccess;

public static class IDbConnectionExtensions
{
    public static Task OpenAsync(this IDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection, nameof(connection));

        if (connection is DbConnection dbConnection)
        {
            return dbConnection.OpenAsync();
        }

        connection.Open();

        return Task.CompletedTask;
    }

    public static async ValueTask<IDbTransaction> BeginTransactionAsync(this IDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection, nameof(connection));

        if (connection is DbConnection dbConnection)
        {
            DbTransaction dbTransaction = await dbConnection.BeginTransactionAsync();

            return dbTransaction;
        }

        IDbTransaction transaction = connection.BeginTransaction();

        return transaction;
    }

    public static ValueTask DisposeAsync(this IDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection, nameof(connection));

        if (connection is DbConnection dbConnection)
        {
            return dbConnection.DisposeAsync();
        }

        connection.Dispose();

        return ValueTask.CompletedTask;
    }
}
