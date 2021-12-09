namespace OutboxSample.Model.Events;

public interface IEvent
{
    Guid Id { get; }
}

public abstract record EnvelopeBase
{
    public abstract Guid EventId { get; }
    public string EventType { get; }
    public Guid AggregateId { get; }
    public string AggregateType { get; }
    public DateTime Timestamp { get; }
    public uint AggregateVersion { get; }
    public uint EventSchemaVersion { get; }

    public EnvelopeBase(string eventType, Guid aggregateId, string aggregateType, DateTime timestamp, uint aggregateVersion, uint eventSchemaVersion)
    {
        EventType = eventType;
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        Timestamp = timestamp;
        AggregateVersion = aggregateVersion;
        EventSchemaVersion = eventSchemaVersion;
    }
}

public record EventEnvelope<TEvent> : EnvelopeBase where TEvent : IEvent
{
    public TEvent Event { get; }

    public override Guid EventId => Event.Id;

    public EventEnvelope(
        TEvent @event,
        string eventType,
        Guid aggregateId,
        string aggregateType,
        DateTime timestamp,
        uint aggregateVersion,
        uint eventSchemaVersion) : base(eventType, aggregateId, aggregateType, timestamp, aggregateVersion, eventSchemaVersion)
    {
        this.Event = @event;
    }
}

public record EventEnvelope : EnvelopeBase
{

    // TODO: this doesn't make a lot of sense if an event is struct
    // TODO: reduce to usage of IEvent, get rid of struct-ased events
    public object Event { get; }

    public override Guid EventId { get; }

    public EventEnvelope(
        object @event,
        Guid eventId,
        string eventType,
        Guid aggregateId,
        string aggregateType,
        DateTime timestamp,
        uint aggregateVersion,
        uint eventSchemaVersion) : base(eventType, aggregateId, aggregateType, timestamp, aggregateVersion, eventSchemaVersion)
    {
        this.Event = @event;
        this.EventId = eventId;
    }
}