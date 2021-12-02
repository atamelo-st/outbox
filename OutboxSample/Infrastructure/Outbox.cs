using OutboxSample.Application;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace OutboxSample.Infrastructure;

public class Outbox : IOutbox
{
    // TODO: remove that!! for testing only!!
    private static readonly Guid aggregateId = Guid.Parse("44e36531-e00f-45a1-9082-98feee02dc95");

    private readonly IConnectionFactory connectionFactory;

    public Outbox(IConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory, nameof(connectionFactory));

        this.connectionFactory = connectionFactory;
    }

    public bool Send<TEvent>(TEvent @event)
    {
        using (IDbConnection connection = this.connectionFactory.GetConnection())
        using (IDbCommand command = connection.CreateCommand())
        {
            command.CommandText = 
@"INSERT INTO outbox_events(id, aggregate_type, aggregate_id, type, payload) VALUES(@EventId, @AggregateType, @AggregateId, @Type, @Payload)";
            command.CommandType = CommandType.Text;

            Guid eventId = Guid.NewGuid();
            command.Parameters.Add(command.CreateParameter("@EventId", eventId, DbType.Guid));

            // TODO: remove! for testing only!
            command.Parameters.Add(command.CreateParameter("@AggregateType", "application-aggregate"));
            command.Parameters.Add(command.CreateParameter("@AggregateId", aggregateId, DbType.Guid));

            // TODO: remove! the 'type' should be derived from the event itself!
            command.Parameters.Add(command.CreateParameter("@Type", "application.user-added"));

            string payload = Serialize(@event);
            command.Parameters.Add(command.CreateParameter("@Payload", payload));

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
