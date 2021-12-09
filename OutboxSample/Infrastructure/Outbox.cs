using OutboxSample.Application;
using OutboxSample.Model.Events;
using System.Data;
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

    public bool Send<TEvent>(EventEnvelope<TEvent> envelope) where TEvent : IEvent
    {
        using IDbConnection connection = this.connectionFactory.GetConnection();
        connection.Open();

        using (IDbTransaction transaction = connection.BeginTransaction())
        using (IDbCommand command = connection.CreateCommand())
        {
            command.Transaction = transaction;

            command.CommandText = 
@"
INSERT INTO outbox_events
    (id, aggregate_type, aggregate_id, type, payload, timestamp, aggregate_version, event_schema_version) 
VALUES
    (@EventId, @AggregateType, @AggregateId, @Type, @Payload, @Timestamp, @AggregateVersion, @EventSchemaVersion)
";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(command.CreateParameter("@EventId", envelope.EventId, DbType.Guid));
            command.Parameters.Add(command.CreateParameter("@AggregateType", envelope.AggregateType));
            command.Parameters.Add(command.CreateParameter("@AggregateId", envelope.AggregateId, DbType.Guid));
            command.Parameters.Add(command.CreateParameter("@Type", envelope.EventType));

            // TODO: easier to serialize with the envelope?
            string payload = Serialize(envelope.Event);
            command.Parameters.Add(command.CreateParameter("@Payload", payload));
            command.Parameters.Add(command.CreateParameter("@Timestamp", envelope.Timestamp));
            command.Parameters.Add(command.CreateParameter("@AggregateVersion", envelope.AggregateVersion));
            command.Parameters.Add(command.CreateParameter("@EventSchemaVersion", envelope.EventSchemaVersion));

            int count = command.ExecuteNonQuery();

            command.CommandText = "DELETE FROM outbox_events WHERE id=@EventId";
            command.ExecuteNonQuery();

            transaction.Commit();

            return count > 0;
        }
    }

    public bool Send<TEvent>(IReadOnlyList<EventEnvelope<TEvent>> events) where TEvent : IEvent
    {
        // TODO: create a transaction
        // TODO: add event deletion after published
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
