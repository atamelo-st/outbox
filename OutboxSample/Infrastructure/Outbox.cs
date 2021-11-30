using OutboxSample.Application;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace OutboxSample.Infrastructure;

public class Outbox : IOutbox
{
    private readonly IConnectionFactory connectionFactory;

    public Outbox(IConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory, nameof(connectionFactory));

        this.connectionFactory = connectionFactory;
    }

    public bool Send<TEvent>(TEvent @event)
    {
        string serialized = Serialize(@event);
        Guid eventId = Guid.NewGuid();

        using (IDbConnection connection = this.connectionFactory.GetConnection())
        using (IDbCommand command = connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO outbox VALUES(@EventId, @Body)";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(command.CreateParameter("@EventId", eventId));
            command.Parameters.Add(command.CreateParameter("@Body", serialized));

            connection.Open();

            int count = command.ExecuteNonQuery();

            return count > 0;
        }
    }

    public bool SendMany<TEvent>(IReadOnlyList<TEvent> events)
    {
        using (IDbConnection connection = this.connectionFactory.GetConnection())
        using (IDbCommand command = connection.CreateCommand())
        {
            var commandText = new StringBuilder("INSERT INTO outbox (eventid, payload) VALUES ");

            for (int i = 0; i < events.Count; i++)
            {
                if (i != 0)
                {
                    commandText.Append(',');
                }
                // TODO: switch to `unnest` when on Postgres? https://github.com/npgsql/npgsql/issues/2779#issuecomment-573439342
                commandText.Append("(@EventId").Append(i).Append(", @Body").Append(i).Append(')');
                command.Parameters.Add(command.CreateParameter($"@EventId{i}", Guid.NewGuid(), DbType.Guid));
                command.Parameters.Add(command.CreateParameter($"@Body{i}", Serialize(events[i]), DbType.String));
            }

            command.CommandType = CommandType.Text;
            command.CommandText = commandText.ToString();

            connection.Open();

            int count = command.ExecuteNonQuery();

            return count > 0;
        }
    }

    private static string Serialize<TEvent>(TEvent @event)
    {
        string json = JsonSerializer.Serialize(@event);

        return json;
    }
}
