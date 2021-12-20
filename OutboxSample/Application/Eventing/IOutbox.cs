using OutboxSample.DomainModel.Events;

namespace OutboxSample.Application.Eventing;

public interface IOutbox : ISupportUnitOfWork
{
    Task<bool> SendAsync(EventEnvelope envelope);

    Task<bool> SendAsync(IReadOnlyList<EventEnvelope> envelopes);
}
