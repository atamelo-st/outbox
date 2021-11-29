namespace OutboxSample.Application;

public interface IOutbox : ISupportUnitOfWork
{
    void Send<TPayload>(EventEnvelope<TPayload> @event);

    public record EventEnvelope<TPayload>
    {
        // TODO: add aggregate type, event type, event id

        public Guid AggregateId { get; }

        public TPayload Payload { get; }

        public DateTime Timestamp { get; }

        public int Version { get; }

        public EventEnvelope(Guid aggregateId, TPayload payload, DateTime timestamp, int version)
        {
            ArgumentNullException.ThrowIfNull(payload, nameof(payload));

            AggregateId = aggregateId;
            Payload = payload;
            Timestamp = timestamp;
            Version = version;
        }
    }
}
