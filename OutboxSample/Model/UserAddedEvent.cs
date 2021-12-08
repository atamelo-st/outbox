namespace OutboxSample.Model;

public readonly record struct UserAddedEvent : IEvent
{
    public Guid UserId { get; }
    public string UserName { get; }

    public Guid Id { get; }

    public UserAddedEvent(Guid Id, Guid userId, string userName)
    {
        ArgumentNullException.ThrowIfNull(userName, nameof(userName));

        this.Id = Id;
        this.UserId = userId;
        this.UserName = userName;
    }

    public UserAddedEvent()
    {
        throw new NotSupportedException("Must use constructor with parameters.");
    }
}

public interface IEvent
{
    Guid Id { get; }
}


public record EventEnvelope<TEvent> where TEvent : IEvent
{
    public TEvent Event { get; }
    public Guid EventId => this.Event.Id;
    public string EventType { get; }
    public Guid AggregateId { get; }
    public string AggregateType { get; }
    public DateTime Timestamp { get; }
    public uint AggregateVersion { get; }
    public uint EventSchemaVersion { get; }

    public EventEnvelope(TEvent @event, string eventType, Guid aggregateId, string aggregateType, DateTime timestamp, uint aggregateVersion, uint eventSchemaVersion)
    {
        this.Event = @event;
        this.EventType = eventType;
        this.AggregateId = aggregateId;
        this.AggregateType = aggregateType;
        this.Timestamp = timestamp;
        this.AggregateVersion = aggregateVersion;
        this.EventSchemaVersion = eventSchemaVersion;
    }
}