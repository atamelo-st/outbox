using OutboxSample.Model.Events;
using System.Collections.Concurrent;

namespace OutboxSample.Application;

public interface IEventMetadataProvider
{
    EventMetadata GetMetadataFor<TEvent>(TEvent @event) where TEvent : IEvent;
}

public class AttrbiuteSourcedEventMetadataProvider : IEventMetadataProvider
{
    private readonly ConcurrentDictionary<Type, EventMetadata> metadataStore;

    public AttrbiuteSourcedEventMetadataProvider()
    {
        this.metadataStore = new ConcurrentDictionary<Type, EventMetadata>();
    }

    public EventMetadata GetMetadataFor<TEvent>(TEvent @event) where TEvent : IEvent
    {
        EventMetadata metadata = this.metadataStore.GetOrAdd(typeof(TEvent), ExtractMetadata);

        return metadata;
    }

    private static EventMetadata ExtractMetadata(Type eventType)
    {
        throw new NotImplementedException();
    }
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
}