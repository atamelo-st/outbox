using System.Data;
using System.Data.Common;

namespace OutboxSample.Infrastructure.DataAccess;

public static class IDbTransactionExtensions
{
    public static Task CommitAsync(this IDbTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

        if (transaction is DbTransaction dbTransaction)
        {
            return dbTransaction.CommitAsync();
        }

        transaction.Commit();

        return Task.CompletedTask;
    }

    public static Task RollbackAsync(this IDbTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

        if (transaction is DbTransaction dbTransaction)
        {
            return dbTransaction.RollbackAsync();
        }

        transaction.Rollback();

        return Task.CompletedTask;
    }

    public static ValueTask DisposeAsync(this IDbTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

        if (transaction is DbTransaction dbTransaction)
        {
            return dbTransaction.DisposeAsync();
        }

        transaction.Dispose();

        return ValueTask.CompletedTask;
    }
}
