using OutboxSample.Application.Eventing;
using OutboxSample.Model.Events;
using System.Collections.Concurrent;

namespace OutboxSample.Infrastructure;

public sealed class AttributeSourcedEventMetadataProvider : IEventMetadataProvider
{
    private readonly ConcurrentDictionary<Type, EventMetadata> metadataStore;

    public AttributeSourcedEventMetadataProvider()
    {
        this.metadataStore = new ConcurrentDictionary<Type, EventMetadata>();
    }

    public EventMetadata GetMetadataFor<TEvent>(TEvent @event) where TEvent : IEvent
    {
        EventMetadata metadata = metadataStore.GetOrAdd(typeof(TEvent), ExtractMetadata);

        return metadata;
    }

    private static EventMetadata ExtractMetadata(Type eventType)
    {
        var metadataAttribute = (EventMetadataAttribute?)Attribute.GetCustomAttribute(eventType, typeof(EventMetadataAttribute));

        if (metadataAttribute is null)
        {
            throw new InvalidOperationException($"Event of type {eventType.FullName} doesn't have metadata attached.");
        }

        return new EventMetadata(metadataAttribute.EventType, metadataAttribute.AggregateType, metadataAttribute.EventSchemaVersion);
    }
}
