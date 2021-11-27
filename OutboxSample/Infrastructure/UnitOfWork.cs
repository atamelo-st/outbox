using Autofac;
using OutboxSample.Application;

namespace OutboxSample.Infrastructure;

public record UnitOfWork : IUnitOfWork
{
    private readonly ILifetimeScope _scope;

    private State _state;


    public UnitOfWork(ILifetimeScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope, nameof(scope));

        this._scope = scope;

        this._state = State.InProgress;
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository, ISupportUnitOfWork
    {
        var repo = this._scope.Resolve<TRepository>();

        return repo;
    }

    public IOutbox GetOutbox()
    {
        var outbox = this._scope.Resolve<IOutbox>();

        return outbox;
    }

    public void Commit()
    {

    }

    public void Rollback()
    {

    }

    public void Dispose()
    {
        this._scope.Dispose();
    }

    private enum State
    {
        Undefined = 0,
        InProgress = 1,
        Comitted = 2,
        Disposed = 3,
    }
}
