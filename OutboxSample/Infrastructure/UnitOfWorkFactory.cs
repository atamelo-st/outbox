using Autofac;
using OutboxSample.Application;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace OutboxSample.Infrastructure;


public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly ILifetimeScope container;

    public UnitOfWorkFactory(ILifetimeScope container)
    {
        this.container = container;
    }

    public IUnitOfWork Begin()
    {
        // 1. Get a live connection to share
        var liveConnectionFactory = this.container.Resolve<IConnectionFactory>();
        IDbConnection liveConnection = liveConnectionFactory.GetConnection();

        // 2. Start a transaction to share - i.e. to 'wrap' the unit of work
        liveConnection.Open();
        IDbTransaction liveTransaction = liveConnection.BeginTransaction();

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

        public IDbConnection GetConnection(string? databaseName = null) => this.sharedConnection;
    }
}

internal class DbConnectionProxy : IDbConnection
{
    private bool hasConnectionHasBeenRequestedToOpen;

    public IDbConnection LiveConnection { get; }

    public DbTransactionProxy TransactionProxy { get; }

    public bool HasTransactionBeenExplicitlyRequested { get; private set; }

    public DbConnectionProxy(IDbConnection liveConnection, DbTransactionProxy transactionProxy)
    {
        ArgumentNullException.ThrowIfNull(liveConnection, nameof(liveConnection));
        ArgumentNullException.ThrowIfNull(transactionProxy, nameof(transactionProxy));

        this.LiveConnection = liveConnection;
        this.TransactionProxy = transactionProxy;

        this.HasTransactionBeenExplicitlyRequested = false;

        this.hasConnectionHasBeenRequestedToOpen = false;
    }

    [NotNull]
    public string? ConnectionString { get => this.LiveConnection.ConnectionString; set => this.LiveConnection.ConnectionString = value; }

    public int ConnectionTimeout => this.LiveConnection.ConnectionTimeout;

    public string Database => this.LiveConnection.Database;

    public ConnectionState State => this.LiveConnection.State;

    // returning a _proxy_ for the ongoing transaction - 
    // this will prevent a client who calls BeginTransaction to call Commit/Rollback/Dispose on a _live_ transaction
    // TODO: implement counter of opening requests?
    public IDbTransaction BeginTransaction()
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
        this.HasTransactionBeenExplicitlyRequested = true;

        return this.TransactionProxy;
    }

    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        this.EnsureHasBeenRequestedToOpen();

        if (this.TransactionProxy.IsolationLevel == il)
        {
            return this.TransactionProxy;
        }

        throw new InvalidOperationException("Can't start a transaction - another transaction is in progress.");
    }

    // TODO: throw on an attempt to change database?
    public void ChangeDatabase(string databaseName) => this.LiveConnection.ChangeDatabase(databaseName);

    public void Close()
    {
        this.hasConnectionHasBeenRequestedToOpen = false;

        // do nothing here - we close the live connection connection only when
        // encompassing scope/unit of work closes
    }

    public IDbCommand CreateCommand()
    {
        IDbCommand liveCommand = this.LiveConnection.CreateCommand();
        liveCommand.Transaction = this.TransactionProxy.LiveTransaction;

        DbCommandProxy command = new (liveCommand, this, this.TransactionProxy);

        return command;
    }

    public void Dispose()
    {
        this.Close();

        // do nothing here - we close the live connection connection only when
        // encompassing scope/unit of work closes
    }

    public void Open()
    {
        // just recording the fact that user 'opened' the connection - for consistency of the API behavior
        this.hasConnectionHasBeenRequestedToOpen = true;

        // do nothing here - live connection opening is mananged outside
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

internal class DbCommandProxy : IDbCommand
{
    private readonly IDbCommand liveCommand;
    private readonly DbConnectionProxy connectionProxy;

    private DbTransactionProxy transactionProxy;


    public DbCommandProxy(IDbCommand liveCommand, DbConnectionProxy connectionProxy, DbTransactionProxy transactionProxy)
    {
        ArgumentNullException.ThrowIfNull(liveCommand, nameof(liveCommand));
        ArgumentNullException.ThrowIfNull(connectionProxy, nameof(connectionProxy));
        ArgumentNullException.ThrowIfNull(transactionProxy, nameof(transactionProxy));

        this.liveCommand = liveCommand;
        this.connectionProxy = connectionProxy;
        this.transactionProxy = transactionProxy;
    }

    [NotNull]
    public string? CommandText { get => this.liveCommand.CommandText; set => this.liveCommand.CommandText = value; }

    public int CommandTimeout { get => this.liveCommand.CommandTimeout; set => this.liveCommand.CommandTimeout = value; }

    public CommandType CommandType { get => this.liveCommand.CommandType; set => this.liveCommand.CommandType = value; }

    // TODO: make the setter a no-op?
    public IDbConnection? Connection { get => this.connectionProxy; set => throw new NotSupportedException("Can't change connection."); }

    public IDataParameterCollection Parameters => this.liveCommand.Parameters;

    // 1. The transaction has already been set upon the command creation
    // 2. The DbConnectionProxy.BeginTransaction is configured to always return the same transaction, which is the same as in #1
    // So, at it's pointless to override the transaction with the same value, command's transaction setter is just a no-op
    public IDbTransaction? Transaction { get => this.transactionProxy; set { } }

    public UpdateRowSource UpdatedRowSource { get => this.liveCommand.UpdatedRowSource; set => this.liveCommand.UpdatedRowSource = value; }

    public void Cancel() => this.liveCommand.Cancel();

    public IDbDataParameter CreateParameter() => this.liveCommand.CreateParameter();

    public void Dispose() => this.liveCommand.Dispose();

    public int ExecuteNonQuery() => this.liveCommand.ExecuteNonQuery();

    public IDataReader ExecuteReader() => this.liveCommand.ExecuteReader();

    public IDataReader ExecuteReader(CommandBehavior behavior) => this.liveCommand.ExecuteReader(behavior);

    public object? ExecuteScalar() => this.ExecuteScalar();

    public void Prepare() => this.liveCommand.Prepare();
}

internal class DbTransactionProxy : IDbTransaction
{
    [NotNull]
    public DbConnectionProxy? ConnectionProxy { get; internal set; }

    public IDbTransaction LiveTransaction { get; }

    public bool HasBeenCommitted { get; private set; }


    public DbTransactionProxy(IDbTransaction liveTransaction)
    {
        ArgumentNullException.ThrowIfNull(liveTransaction, nameof(liveTransaction));

        this.LiveTransaction = liveTransaction;

        this.HasBeenCommitted = false;
    }

    public IDbConnection? Connection => this.ConnectionProxy;
    public IsolationLevel IsolationLevel => this.LiveTransaction.IsolationLevel;

    public void Commit()
    {
        this.HasBeenCommitted = true;

        // do nothing - if there a transaction proxy, then there is a UoW transaction is in progress
        // it'll be committed when the UoW commits
    }

    public void Dispose()
    {
        // do nothing - it's a proxy
    }

    public void Rollback()
    {
        if (this.HasBeenCommitted)
        {
            throw new InvalidOperationException("Can't roll back - the transaction has already been committed.");
        }

        // do nothing - it's a proxy
    }
}