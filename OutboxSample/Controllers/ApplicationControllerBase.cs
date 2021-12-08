using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;
using OutboxSample.Model.Events;

namespace OutboxSample.Controllers;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected IEventMetadataProvider EventMetadataProvider { get; }
    protected ITimeProvider TimeProvider { get; }

    protected ApplicationControllerBase(IEventMetadataProvider eventMetadataProvider, ITimeProvider timeProvider)
    {
        this.EventMetadataProvider = eventMetadataProvider;
        this.TimeProvider = timeProvider;
    }

    protected EventEnvelope<TEvent> WrapEvent<TEvent>(
        TEvent @event,
        Guid aggregateId,
        uint aggregateVersion
    ) where TEvent : IEvent
    {
        EventMetadata eventMetadata = this.EventMetadataProvider.GetMetadataFor(@event);

        DateTime timestamp = this.TimeProvider.Now;

        return new EventEnvelope<TEvent>(
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
