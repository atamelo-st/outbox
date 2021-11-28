using Autofac;
using OutboxSample.Application;
using System.Data;

namespace OutboxSample.Infrastructure;

public record UnitOfWork : IUnitOfWork
{
    private readonly ILifetimeScope dependencyResolver;
    private readonly IDbConnection connection;
    private readonly IDbTransaction transaction;

    private State state;


    public UnitOfWork(ILifetimeScope dependencyResolver, IDbConnection connection, IDbTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(dependencyResolver, nameof(dependencyResolver));
        ArgumentNullException.ThrowIfNull(connection, nameof(connection));
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

        this.dependencyResolver = dependencyResolver;
        this.connection = connection;
        this.transaction = transaction;
        this.state = State.InProgress;
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository, ISupportUnitOfWork
    {
        var repo = this.dependencyResolver.Resolve<TRepository>();

        return repo;
    }

    public IOutbox GetOutbox()
    {
        var outbox = this.dependencyResolver.Resolve<IOutbox>();

        return outbox;
    }

    public void Commit()
    {
        this.transaction.Commit();

        this.state = State.Comitted;
    }

    public void Rollback()
    {
        this.transaction.Rollback();

        this.state = State.RolledBack;
    }

    public void Dispose()
    {
        if (this.state == State.InProgress)
        {
            this.transaction.Rollback();
        }

        this.transaction.Dispose();
        this.connection.Dispose();
        this.dependencyResolver.Dispose();
    }

    private enum State
    {
        Undefined = 0,
        InProgress = 1,
        Comitted = 2,
        RolledBack = 3,
        Disposed = 4,
    }
}
