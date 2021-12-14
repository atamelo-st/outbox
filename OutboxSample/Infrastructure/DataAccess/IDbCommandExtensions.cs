using System.Data;
using System.Data.Common;

namespace OutboxSample.Infrastructure.DataAccess;

public static class IDbCommandExtensions
{
    public static DbParameter CreateParameter(this IDbCommand command, string parameterName, DbType dbType, int size)
    {
        DbParameter parameter = command.CreateParameter(parameterName, dbType);

        parameter.Size = size;

        return parameter;
    }

    public static DbParameter CreateParameter(this IDbCommand command, string parameterName, DbType dbType)
        => command.CreateParameter(parameterName, null, dbType);

    public static DbParameter CreateParameter(this IDbCommand command, string parameterName, object? parameterValue, DbType dbType)
    {
        DbParameter parameter = command.CreateParameter(parameterName, parameterValue);

        parameter.DbType = dbType;

        return parameter;
    }

    // TODO: when switched to Postgres provider, user strongly typed parameter creation method to avoid parameter value boxing
    public static DbParameter CreateParameter(this IDbCommand command, string parameterName, object? parameterValue)
    {
        var parameter = (DbParameter)command.CreateParameter();

        parameter.ParameterName = parameterName;
        parameter.Value = parameterValue;

        return parameter;
    }

    public static Task<int> ExecuteNonQueryAsync(this IDbCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        command = command.GetLiveCommand();

        if (command is DbCommand dbCommand)
        {
            return dbCommand.ExecuteNonQueryAsync();
        }

        int count = command.ExecuteNonQuery();

        return Task.FromResult(count);
    }

    public static async Task<IDataReader> ExecuteReaderAsync(this IDbCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        command = command.GetLiveCommand();

        if (command is DbCommand dbCommand)
        {
            DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync();

            return dbDataReader;
        }

        IDataReader dataReader = command.ExecuteReader();

        return dataReader;
    }

    public static Task<object?> ExecuteScalarAsync(this IDbCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        command = command.GetLiveCommand();

        if (command is DbCommand dbCommand)
        {
            return dbCommand.ExecuteScalarAsync();
        }

        object? result = command.ExecuteScalar();

        return Task.FromResult(result);
    }

    public static Task PrepareAsync(this IDbCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        command = command.GetLiveCommand();

        if (command is DbCommand dbCommand)
        {
            return dbCommand.PrepareAsync();
        }

        command.Prepare();

        return Task.CompletedTask;
    }

    private static IDbCommand GetLiveCommand(this IDbCommand command)
    {
        if (command is IDbCommandProxy proxy)
        {
            return proxy.LiveCommand;
        }

        return command;
    }
}