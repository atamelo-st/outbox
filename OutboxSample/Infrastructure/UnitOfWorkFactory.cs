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
        var liveConnectionFactory = this.container.Resolve<IConnectionFactory>();
        IDbConnection liveConnection = liveConnectionFactory.GetConnection();

        liveConnection.Open();
        IDbTransaction transaction = liveConnection.BeginTransaction();

        var sharedConnection = new SharedConnectionProxy(liveConnection, transaction);
        var sharedConnectionFactory = new SharedConnectionFactory(sharedConnection);

        ILifetimeScope scopeContainer = this.container.BeginLifetimeScope(
          scopeBuilder =>
          {
              // overriding default connection factory with transactional one
              scopeBuilder.Register(_ => sharedConnectionFactory).As<IConnectionFactory>().InstancePerLifetimeScope();
          }
        );

        Debug.Assert(scopeContainer.Resolve<IConnectionFactory>().GetType() == typeof(SharedConnectionFactory));
 
        var unitOfWork = new UnitOfWork(scopeContainer, liveConnection, transaction);

        return unitOfWork;
    }

    private class SharedConnectionFactory : IConnectionFactory
    {
        private readonly SharedConnectionProxy sharedConnection;

        public SharedConnectionFactory(SharedConnectionProxy sharedConnection)
        {
            ArgumentNullException.ThrowIfNull(sharedConnection, nameof(sharedConnection));

            this.sharedConnection = sharedConnection;
        }

        public IDbConnection GetConnection() => this.sharedConnection;
    }

    private class SharedConnectionProxy : IDbConnection
    {
        private readonly IDbConnection liveConnection;
        private readonly IDbTransaction ongoingTransaction;

        public string ConnectionString
        {
            get => this.liveConnection.ConnectionString;
            [param: NotNull]
            set => this.liveConnection.ConnectionString = value;
        }

        public int ConnectionTimeout => this.liveConnection.ConnectionTimeout;

        public string Database => this.liveConnection.Database;

        public ConnectionState State => this.liveConnection.State;

        public SharedConnectionProxy(IDbConnection liveConnection, IDbTransaction ongoingTransaction)
        {
            ArgumentNullException.ThrowIfNull(liveConnection, nameof(liveConnection));
            ArgumentNullException.ThrowIfNull(ongoingTransaction, nameof(ongoingTransaction));

            this.liveConnection = liveConnection;
            this.ongoingTransaction = ongoingTransaction;
        }

        public IDbTransaction BeginTransaction() => this.ongoingTransaction;

        public IDbTransaction BeginTransaction(IsolationLevel il) => throw new InvalidOperationException("Not supposed to call this.");

        public void ChangeDatabase(string databaseName) => this.liveConnection.ChangeDatabase(databaseName);

        public void Close()
        {
            // do nothing here - we close the live connection connection only when
            // encompassing scope/unit of work closes
        }

        public IDbCommand CreateCommand()
        {
            IDbCommand command = this.liveConnection.CreateCommand();
            command.Transaction = this.ongoingTransaction;

            return command;
        }

        public void Dispose()
        {
            // do nothing here - we close the live connection connection only when
            // encompassing scope/unit of work closes
        }

        public void Open()
        {
            // do nothing here - live connection opening is mananged outside
        }
    }
}
