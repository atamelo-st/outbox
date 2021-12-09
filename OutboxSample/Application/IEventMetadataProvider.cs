using OutboxSample.Model.Events;

namespace OutboxSample.Application;

public interface IEventMetadataProvider
{
    EventMetadata GetMetadataFor<TEvent>(TEvent @event) where TEvent : IEvent;
}

public record struct EventMetadata
{
    public string EventType { get; }
    public string AgregateType { get; }
    public uint EventSchemaVersion { get; }

    public EventMetadata(string eventType, string agregateType, uint eventSchemaVersion)
    {
        ArgumentNullException.ThrowIfNull(eventType, nameof(eventType));
        ArgumentNullException.ThrowIfNull(agregateType, nameof(agregateType));

        EventType = eventType;
        AgregateType = agregateType;
        EventSchemaVersion = eventSchemaVersion;
    }

    public EventMetadata()
    {
        throw new InvalidOperationException("Use constructor with parameters.");
    }
}