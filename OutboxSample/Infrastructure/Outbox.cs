using OutboxSample.Application;

namespace OutboxSample.Infrastructure;

public class Outbox : IOutbox
{
    private readonly IConnectionFactory connectionFactory;

    public Outbox(IConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory, nameof(connectionFactory));

        this.connectionFactory = connectionFactory;
    }

    public void Send<TEvent>(TEvent @event)
    {
        throw new NotImplementedException();
    }
}
