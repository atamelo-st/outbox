using Autofac;
using OutboxSample.Application;

namespace OutboxSample.Infrastructure;

public record AutofacBackedUnitOfWork : IUnitOfWork
{
    private readonly ILifetimeScope scope;

    private State _state;


    public AutofacBackedUnitOfWork(ILifetimeScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope, nameof(scope));

        this.scope = scope;

        this._state = State.InProgress;
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository, ISupportUnitOfWork
    {
        throw new NotImplementedException();
    }

    public IOutbox GetOutbox()
    {
        throw new NotImplementedException();
    }

    public void Commit()
    {

    }

    public void Rollback()
    {

    }

    public void Dispose()
    {

    }

    private enum State
    {
        Undefined = 0,
        InProgress = 1,
        Comitted = 2,
        Disposed = 3,
    }
}
