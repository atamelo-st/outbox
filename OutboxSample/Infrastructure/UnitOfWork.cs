using Autofac;
using OutboxSample.Application;
using OutboxSample.Application.DataAccess;
using OutboxSample.Application.Eventing;

namespace OutboxSample.Infrastructure;

internal record UnitOfWork : IUnitOfWork
{
    private readonly ILifetimeScope dependencyResolver;
    private readonly DbConnectionProxy connectionProxy;

    private State state;

    public UnitOfWork(ILifetimeScope dependencyResolver, DbConnectionProxy connectionProxy)
    {
        ArgumentNullException.ThrowIfNull(dependencyResolver, nameof(dependencyResolver));
        ArgumentNullException.ThrowIfNull(connectionProxy, nameof(connectionProxy));

        this.dependencyResolver = dependencyResolver;
        this.connectionProxy = connectionProxy;

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

    public async Task<bool> CommitAsync()
    {
        // in order to commit the _live_ transaction, the following prerequisite should hold:
        // the transaction proxy from ConnectionProxy.BeginTransaction either hasn't been requested
        // or has been requested AND has been committed
        bool shouldCommit =
            this.connectionProxy.HasTransactionBeenExplicitlyRequested is not true ||
            this.connectionProxy.TransactionProxy.HasBeenCommitted;

        if (shouldCommit)
        {
            await this.connectionProxy.TransactionProxy.LiveTransaction.CommitAsync();

            this.state = State.Committed;

            return true;
        }

        await this.RollbackAsync();

        return false;
    }

    public async Task RollbackAsync()
    {
        await this.connectionProxy.TransactionProxy.LiveTransaction.RollbackAsync();

        this.state = State.RolledBack;
    }

    public async ValueTask DisposeAsync()
    {
        if (this.state == State.Disposed)
        {
            return;
        }

        if (this.state == State.InProgress)
        {
            await this.RollbackAsync();
        }

        await this.connectionProxy.TransactionProxy.LiveTransaction.DisposeAsync();
        await this.connectionProxy.LiveConnection.DisposeAsync();
        await this.dependencyResolver.DisposeAsync();

        this.state = State.Disposed;
    }

    private enum State
    {
        Undefined = 0,
        InProgress = 1,
        Committed = 2,
        RolledBack = 3,
        Disposed = 4,
    }
}
