using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;
using OutboxSample.Model;

namespace OutboxSample.Controllers;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected IUnitOfWorkFactory UnitOfWork { get; }
    protected ITimeProvider TimeProvider { get; }

    protected ApplicationControllerBase(IUnitOfWorkFactory unitOfWorkFactory, ITimeProvider timeProvider)
    {
        this.UnitOfWork = unitOfWorkFactory;
        this.TimeProvider = timeProvider;
    }

    protected EventEnvelope<TEvent> WrapEvent<TEvent>(
        TEvent @event,
        Guid aggregateId,
        uint aggregateVersion
    ) where TEvent : IEvent
    {
        // TODO: infer from event type
        string eventType = "";
        string aggregateType = "";
        uint eventSchemaVersion = 0;

        DateTime timestamp = this.TimeProvider.Now;

        return new EventEnvelope<TEvent>(@event, eventType, aggregateId, aggregateType, timestamp, aggregateVersion, eventSchemaVersion);
    }
}
