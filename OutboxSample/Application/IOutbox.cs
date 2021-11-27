using OutboxSample.Model;

namespace OutboxSample.Application;

public interface IOutbox : ISupportUnitOfWork
{
    void Publish<TPayload>(EventEnvelope<TPayload> @event);
}
