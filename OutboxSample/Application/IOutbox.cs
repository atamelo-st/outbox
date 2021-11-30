namespace OutboxSample.Application;

public interface IOutbox : ISupportUnitOfWork
{
    bool Send<TEvent>(TEvent @event);

    bool Send<TEvent>(IEnumerable<TEvent> events);
}
