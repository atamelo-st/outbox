using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application.Eventing;
using OutboxSample.Common;
using OutboxSample.Model.Events;

namespace OutboxSample.Presentation;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected IEventMetadataProvider EventMetadataProvider { get; }
    protected ITimeProvider TimeProvider { get; }

    protected ApplicationControllerBase(IEventMetadataProvider eventMetadataProvider, ITimeProvider timeProvider)
    {
        EventMetadataProvider = eventMetadataProvider;
        TimeProvider = timeProvider;
    }

    protected EventEnvelope WrapEvent<TEvent>(
        TEvent @event,
        Guid aggregateId,
        uint aggregateVersion
    ) where TEvent : IEvent
    {
        EventMetadata eventMetadata = EventMetadataProvider.GetMetadataFor(@event);

        DateTime timestamp = TimeProvider.UtcNow;

        return new EventEnvelope(
            // NOTE: if TEvent is a struct, this is where boxing will happen
            @event,
            eventMetadata.EventType,
            aggregateId,
            eventMetadata.AgregateType,
            timestamp,
            aggregateVersion,
            eventMetadata.EventSchemaVersion
        );
    }
}
