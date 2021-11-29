using System.Data;
using System.Data.Common;

namespace OutboxSample.Infrastructure;

public static class IDbCommandExtensions
{
    public static DbParameter CreateParameter(this IDbCommand command, string parameterName, object? parameterValue)
    {
        var parameter =  (DbParameter)command.CreateParameter();

        parameter.ParameterName = parameterName;
        parameter.Value = parameterValue;

        return parameter;
    }
}