using Autofac;
using OutboxSample.Application;
using OutboxSample.Application.DataAccess;
using OutboxSample.Application.Eventing;
using System.Data;

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

    public bool Commit()
    {
        // in order to commit the _live_ transaction, the following prerequisite should hold:
        // the transaction proxy from ConnectionProxy.BeginTransaction either hasn't been requested
        // or has been requested AND has been committed
        bool shouldCommit =
            this.connectionProxy.HasTransactionBeenExplicitlyRequested is not true ||
            this.connectionProxy.TransactionProxy.HasBeenCommitted;

        if (shouldCommit)
        {
            this.connectionProxy.TransactionProxy.LiveTransaction.Commit();

            this.state = State.Comitted;

            return true;
        }

        this.Rollback();

        return false;
    }

    public void Rollback()
    {
        this.connectionProxy.TransactionProxy.LiveTransaction.Rollback();

        this.state = State.RolledBack;
    }

    public void Dispose()
    {
        if (this.state == State.Disposed)
        {
            return;
        }

        if (this.state == State.InProgress)
        {
            this.Rollback();
        }

        this.connectionProxy.TransactionProxy.LiveTransaction.Dispose();
        this.connectionProxy.LiveConnection.Dispose();
        this.dependencyResolver.Dispose();

        this.state = State.Disposed;
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
