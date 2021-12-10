using OutboxSample.Model.Events;

namespace OutboxSample.Application.Eventing;

public interface IOutbox : ISupportUnitOfWork
{
    bool Send(EventEnvelope envelope);

    bool Send(IReadOnlyList<EventEnvelope> envelopes);
}
