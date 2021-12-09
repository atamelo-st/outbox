namespace OutboxSample.Model.Events;

public interface IEvent
{
    Guid Id { get; }
}

// NOTE: this is to reprenent an envelope that can be put into a collection of event envelopes
// As such collection may contain events of different types, this envelope cannot hava a concrete event type specified
// Hence the use of IEvent vs <TEvent> where TEvent : IEvent
public record EventEnvelope
{
    public IEvent Event { get; }
    public Guid EventId => Event.Id;
    public string EventType { get; }
    public Guid AggregateId { get; }
    public string AggregateType { get; }
    public DateTime Timestamp { get; }
    public uint AggregateVersion { get; }
    public uint EventSchemaVersion { get; }

    public EventEnvelope(
        IEvent @event,
        string eventType,
        Guid aggregateId,
        string aggregateType,
        DateTime timestamp,
        uint aggregateVersion,
        uint eventSchemaVersion
    )
    {
        ArgumentNullException.ThrowIfNull(@event, nameof(@event));
        ArgumentNullException.ThrowIfNull(eventType, nameof(eventType));
        ArgumentNullException.ThrowIfNull(aggregateType, nameof(aggregateType));

        this.Event = @event;
        this.EventType = eventType;
        this.AggregateId = aggregateId;
        this.AggregateType = aggregateType;
        this.Timestamp = timestamp;
        this.AggregateVersion = aggregateVersion;
        this.EventSchemaVersion = eventSchemaVersion;
    }
}
