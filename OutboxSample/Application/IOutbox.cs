using OutboxSample.Model;

namespace OutboxSample.Application;

public interface IOutbox : ISupportUnitOfWork
{
    bool Send<TEvent>(EventEnvelope<TEvent> envelope) where TEvent : IEvent;

    bool SendMany<TEvent>(IReadOnlyList<TEvent> events);
}
