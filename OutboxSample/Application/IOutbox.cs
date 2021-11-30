namespace OutboxSample.Application;

public interface IOutbox : ISupportUnitOfWork
{
    bool Send<TEvent>(TEvent @event);

    bool SendMany<TEvent>(IReadOnlyList<TEvent> events);
}
