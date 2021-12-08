namespace OutboxSample.Model.Events;

public interface IEvent
{
    Guid Id { get; }
}

public record EventEnvelope<TEvent> where TEvent : IEvent
{
    public TEvent Event { get; }
    public Guid EventId => Event.Id;
    public string EventType { get; }
    public Guid AggregateId { get; }
    public string AggregateType { get; }
    public DateTime Timestamp { get; }
    public uint AggregateVersion { get; }
    public uint EventSchemaVersion { get; }

    public EventEnvelope(TEvent @event, string eventType, Guid aggregateId, string aggregateType, DateTime timestamp, uint aggregateVersion, uint eventSchemaVersion)
    {
        Event = @event;
        EventType = eventType;
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        Timestamp = timestamp;
        AggregateVersion = aggregateVersion;
        EventSchemaVersion = eventSchemaVersion;
    }
}