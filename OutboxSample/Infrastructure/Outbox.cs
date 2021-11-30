using OutboxSample.Application;
using System.Data;
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

    public bool Send<TEvent>(IEnumerable<TEvent> events)
    {
        // TODO: try BatchCommand/SQL `unnest` function
        throw new NotImplementedException();
    }

    private static string Serialize<TEvent>(TEvent @event)
    {
        string json = JsonSerializer.Serialize(@event);

        return json;
    }
}
