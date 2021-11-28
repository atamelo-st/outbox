using Autofac;
using OutboxSample.Application;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace OutboxSample.Infrastructure;


public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly ILifetimeScope _container;

    public UnitOfWorkFactory(ILifetimeScope container)
    {
        this._container = container;
    }

    public IUnitOfWork Begin()
    {
        // TODO: move this all to scopeBuilder.Register?
        var globalConnectionFactory = this._container.Resolve<IConnectionFactory>();
        IDbConnection liveConnection = globalConnectionFactory.GetConnection();
        liveConnection.Open();
        IDbTransaction transaction = liveConnection.BeginTransaction();
        var sharedConnection = new SharedConnectionProxy(liveConnection, transaction);
        var sharedConnectionFactory = new SharedConnectionFactory(sharedConnection);

        ILifetimeScope scopeContainer = this._container.BeginLifetimeScope(
        // overriding default connection factory with transactional one
          scopeBuilder => scopeBuilder.Register(_ => sharedConnectionFactory).As<IConnectionFactory>().InstancePerLifetimeScope()
        );

        Debug.Assert(scopeContainer.Resolve<IConnectionFactory>().GetType() == typeof(SharedConnectionFactory));
 
        var unitOfWork = new UnitOfWork(scopeContainer, liveConnection, transaction);

        return unitOfWork;
    }

    private class SharedConnectionFactory : IConnectionFactory
    {
        private readonly SharedConnectionProxy _sharedConnection;

        public SharedConnectionFactory(SharedConnectionProxy sharedConnection)
        {
            ArgumentNullException.ThrowIfNull(sharedConnection, nameof(sharedConnection));

            this._sharedConnection = sharedConnection;
        }

        public IDbConnection GetConnection() => this._sharedConnection;
    }

    private class SharedConnectionProxy : IDbConnection
    {
        private readonly IDbConnection _liveConnection;
        private readonly IDbTransaction _ambientTransaction;

        public string ConnectionString
        {
            get => this._liveConnection.ConnectionString;
            [param: NotNull]
            set => this._liveConnection.ConnectionString = value;
        }

        public int ConnectionTimeout => this._liveConnection.ConnectionTimeout;

        public string Database => this._liveConnection.Database;

        public ConnectionState State => this._liveConnection.State;

        public SharedConnectionProxy(IDbConnection innerConnection, IDbTransaction ambientTransaction)
        {
            ArgumentNullException.ThrowIfNull(innerConnection, nameof(innerConnection));
            ArgumentNullException.ThrowIfNull(ambientTransaction, nameof(ambientTransaction));

            this._liveConnection = innerConnection;
            this._ambientTransaction = ambientTransaction;
        }

        public IDbTransaction BeginTransaction() => this._ambientTransaction;

        public IDbTransaction BeginTransaction(IsolationLevel il) => throw new InvalidOperationException("Not supposed to call this.");

        public void ChangeDatabase(string databaseName) => this._liveConnection.ChangeDatabase(databaseName);

        public void Close()
        {
            // do nothing here - we close the live connection connection only when
            // encompassing scope/unit of work closes
        }

        public IDbCommand CreateCommand()
        {
            IDbCommand command = this._liveConnection.CreateCommand();
            command.Transaction = this._ambientTransaction;

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
