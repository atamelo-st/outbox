using System.Data;
using System.Data.Common;

namespace OutboxSample.Infrastructure;

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
        var parameter =  (DbParameter)command.CreateParameter();

        parameter.ParameterName = parameterName;
        parameter.Value = parameterValue;

        return parameter;
    }
}