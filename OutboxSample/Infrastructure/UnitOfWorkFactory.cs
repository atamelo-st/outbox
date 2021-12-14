using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Autofac;

using OutboxSample.Application;
using OutboxSample.Infrastructure.DataAccess;

namespace OutboxSample.Infrastructure;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly ILifetimeScope container;

    public UnitOfWorkFactory(ILifetimeScope container)
    {
        ArgumentNullException.ThrowIfNull(container, nameof(container));

        this.container = container;
    }

    public async Task<IUnitOfWork> BeginAsync()
    {
        // 1. Get a live connection to share
        var liveConnectionFactory = this.container.Resolve<IConnectionFactory>();
        DbConnection liveConnection = liveConnectionFactory.GetConnection();

        // 2. Start a transaction to share - i.e. to 'wrap' the unit of work
        await liveConnection.OpenAsync();
        DbTransaction liveTransaction = await liveConnection.BeginTransactionAsync();

        // 3. Wire up the connection/transaction sharing logic
        DbTransactionProxy transactionProxy = new (liveTransaction);
        DbConnectionProxy connectionProxy = new (liveConnection, transactionProxy);
        transactionProxy.ConnectionProxy = connectionProxy;
        SharedConnectionFactory sharedConnectionFactory = new (connectionProxy);

        // 4. Create isolated dependency scope to manage UoW's dependencies
        ILifetimeScope scopeContainer = this.container.BeginLifetimeScope(
          scopeBuilder =>
          {
              // 5. Override default connection factory with newly created transactional one
              scopeBuilder.Register(_ => sharedConnectionFactory).As<IConnectionFactory>().InstancePerLifetimeScope();
          }
        );

        Debug.Assert(scopeContainer.Resolve<IConnectionFactory>().GetType() == typeof(SharedConnectionFactory));

        UnitOfWork? unitOfWork = new (scopeContainer, connectionProxy);

        return unitOfWork;
    }

    private class SharedConnectionFactory : IConnectionFactory
    {
        private readonly DbConnectionProxy sharedConnection;

        public SharedConnectionFactory(DbConnectionProxy sharedConnection)
        {
            ArgumentNullException.ThrowIfNull(sharedConnection, nameof(sharedConnection));

            this.sharedConnection = sharedConnection;
        }

        public DbConnection GetConnection(string? databaseName = null) => this.sharedConnection;
    }
}

internal class DbConnectionProxy : DbConnection
{
    private bool hasConnectionHasBeenRequestedToOpen;

    public DbConnection LiveConnection { get; }

    public DbTransactionProxy TransactionProxy { get; }

    public bool HasTransactionBeenExplicitlyRequested { get; private set; }

    public DbConnectionProxy(DbConnection liveConnection, DbTransactionProxy transactionProxy)
    {
        ArgumentNullException.ThrowIfNull(liveConnection, nameof(liveConnection));
        ArgumentNullException.ThrowIfNull(transactionProxy, nameof(transactionProxy));

        this.LiveConnection = liveConnection;
        this.TransactionProxy = transactionProxy;

        this.HasTransactionBeenExplicitlyRequested = false;

        this.hasConnectionHasBeenRequestedToOpen = false;
    }

    [NotNull]
    public override string? ConnectionString { get => this.LiveConnection.ConnectionString; set => this.LiveConnection.ConnectionString = value; }

    public override int ConnectionTimeout => this.LiveConnection.ConnectionTimeout;

    public override string Database => this.LiveConnection.Database;

    public override ConnectionState State => this.LiveConnection.State;

    public override string DataSource => this.LiveConnection.DataSource;

    public override string ServerVersion => this.LiveConnection.ServerVersion;

    // returning a _proxy_ for the ongoing transaction - 
    // this will prevent a client who calls BeginTransaction to call Commit/Rollback/Dispose on a _live_ transaction
    // TODO: implement counter of opening requests?
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        this.EnsureHasBeenRequestedToOpen();

        // TODO: `HasTransactionBeenExplicitlyRequested = true;` is not enough as a check - we need to mantain a counter.
        // Plus, if we want to implement 'correct' repository usage - i.e. no transactions across repositories
        // (and we should have 1 repo per an aggregate root) as an aggregate root is a consistency boundary itself -
        // we should allow for the .BeginTransaction() to be called 2 times _tops_ - 1 time for inside the outbox implementation
        // and 1 time inside a repo.
        // Another, arguably better way to enfore hte rule "1 UoW means only 1 repo" would be in UnitOfWork.GetRepository

        // if a transaction has been explicitly requested by the user via .BeginTrnasaction
        // they are now supposed to commit/rollack it.
        // Otherwise, the UoW will rollback the 'enclosing' live transaction in UnitOfWork.Commit()/Dispose().
        if (isolationLevel == IsolationLevel.Unspecified || this.TransactionProxy.IsolationLevel == isolationLevel)
        {
            this.HasTransactionBeenExplicitlyRequested = true;
            return this.TransactionProxy;
        }

        throw new InvalidOperationException("Can't start a transaction - another transaction is in progress.");
    }

    protected override ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(this.BeginDbTransaction(isolationLevel));
    }

    // TODO: throw on an attempt to change database?
    public override void ChangeDatabase(string databaseName) => this.LiveConnection.ChangeDatabase(databaseName);

    public override Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default) 
        => this.LiveConnection.ChangeDatabaseAsync(databaseName, cancellationToken);

    public override void Close()
    {
        this.hasConnectionHasBeenRequestedToOpen = false;

        // do nothing here - we close the live connection connection only when
        // encompassing scope/unit of work closes
    }

    public override Task CloseAsync()
    {
        this.Close();

        // do nothing here - we close the live connection connection only when
        // encompassing scope/unit of work closes

        return Task.CompletedTask;
    }

    protected override DbCommand CreateDbCommand()
    {
        DbCommand liveCommand = this.LiveConnection.CreateCommand();
        liveCommand.Transaction = this.TransactionProxy.LiveTransaction;

        DbCommandProxy command = new (liveCommand, this, this.TransactionProxy);

        return command;
    }

    protected override void Dispose(bool disposing)
    {
        this.Close();

        // do nothing here - we close the live connection connection only when
        // encompassing scope/unit of work closes
    }

    public override ValueTask DisposeAsync()
    {
        this.Close();

        return ValueTask.CompletedTask;
    }

    public override void Open()
    {
        // just recording the fact that user 'opened' the connection - for consistency of the API behavior
        this.hasConnectionHasBeenRequestedToOpen = true;

        // do nothing here - live connection opening is mananged outside
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        this.Open();

        // do nothing here - live connection opening is mananged outside
        return Task.CompletedTask;
    }

    private void EnsureHasBeenRequestedToOpen()
    {
        if (this.hasConnectionHasBeenRequestedToOpen is not true)
        {
            // TODO: check exception type and messaging
            throw new InvalidOperationException("Can't start a transaction on a closed connection.");
        }
    }
}

internal class DbCommandProxy : DbCommand
{
    private readonly DbCommand liveCommand;
    private readonly DbConnectionProxy connectionProxy;
    private readonly DbTransactionProxy transactionProxy;

    public DbCommandProxy(DbCommand liveCommand, DbConnectionProxy connectionProxy, DbTransactionProxy transactionProxy)
    {
        ArgumentNullException.ThrowIfNull(liveCommand, nameof(liveCommand));
        ArgumentNullException.ThrowIfNull(connectionProxy, nameof(connectionProxy));
        ArgumentNullException.ThrowIfNull(transactionProxy, nameof(transactionProxy));

        this.liveCommand = liveCommand;
        this.connectionProxy = connectionProxy;
        this.transactionProxy = transactionProxy;
    }

    [NotNull]
    public override string? CommandText { get => this.liveCommand.CommandText; set => this.liveCommand.CommandText = value; }

    public override int CommandTimeout { get => this.liveCommand.CommandTimeout; set => this.liveCommand.CommandTimeout = value; }

    public override CommandType CommandType { get => this.liveCommand.CommandType; set => this.liveCommand.CommandType = value; }

    // TODO: make the setter a no-op?
    protected override DbConnection? DbConnection { get => this.connectionProxy; set => throw new NotSupportedException("Can't change connection."); }

    protected override DbParameterCollection DbParameterCollection => this.liveCommand.Parameters;

    // 1. The transaction has already been set upon the command creation
    // 2. The DbConnectionProxy.BeginTransaction is configured to always return the same transaction, which is the same as in #1
    // So, at it's pointless to override the transaction with the same value, command's transaction setter is just a no-op
    protected override DbTransaction? DbTransaction { get => this.transactionProxy; set { } }

    public override UpdateRowSource UpdatedRowSource { get => this.liveCommand.UpdatedRowSource; set => this.liveCommand.UpdatedRowSource = value; }

    public override bool DesignTimeVisible { get => this.liveCommand.DesignTimeVisible; set => this.liveCommand.DesignTimeVisible = value; }

    public override void Cancel() => this.liveCommand.Cancel();

    protected override DbParameter CreateDbParameter() => this.liveCommand.CreateParameter();

    protected override void Dispose(bool disposing) => this.liveCommand.Dispose();

    public override ValueTask DisposeAsync() => this.liveCommand.DisposeAsync();

    public override int ExecuteNonQuery() => this.liveCommand.ExecuteNonQuery();

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => this.liveCommand.ExecuteNonQueryAsync(cancellationToken);

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => this.liveCommand.ExecuteReader(behavior);

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        => this.liveCommand.ExecuteReaderAsync(behavior, cancellationToken);

    public override object? ExecuteScalar() => this.liveCommand.ExecuteScalar();

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken) => this.liveCommand.ExecuteScalarAsync(cancellationToken);

    public override void Prepare() => this.liveCommand.Prepare();

    public override Task PrepareAsync(CancellationToken cancellationToken = default) => this.liveCommand.PrepareAsync(cancellationToken);
}

internal class DbTransactionProxy : DbTransaction
{
    [NotNull]
    public DbConnectionProxy? ConnectionProxy { get; internal set; }

    public DbTransaction LiveTransaction { get; }

    public bool HasBeenCommitted { get; private set; }

    public DbTransactionProxy(DbTransaction liveTransaction)
    {
        ArgumentNullException.ThrowIfNull(liveTransaction, nameof(liveTransaction));

        this.LiveTransaction = liveTransaction;

        this.HasBeenCommitted = false;
    }

    public override IsolationLevel IsolationLevel => this.LiveTransaction.IsolationLevel;

    protected override DbConnection? DbConnection => this.ConnectionProxy;
    
    public override void Commit()
    {
        this.HasBeenCommitted = true;

        // do nothing - if there a transaction proxy, then there is a UoW transaction is in progress
        // it'll be committed when the UoW commits
    }

    public override Task CommitAsync(CancellationToken cancellationToken = default)
    {
        this.Commit();

        return Task.CompletedTask;
    }

    public override ValueTask DisposeAsync()
    {
        // do nothing - it's a proxy

        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        // do nothing - it's a proxy
    }

    public override void Rollback()
    {
        if (this.HasBeenCommitted)
        {
            throw new InvalidOperationException("Can't roll back - the transaction has already been committed.");
        }

        // do nothing - it's a proxy
    }

    public override Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        this.Rollback();

        return Task.CompletedTask;
    }
}