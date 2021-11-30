namespace OutboxSample.Application;

public interface IOutbox : ISupportUnitOfWork
{
    void Send<TEvent>(TEvent @event);
}
