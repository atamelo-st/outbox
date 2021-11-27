using Autofac;
using OutboxSample.Application;
using System.Diagnostics;

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
        ILifetimeScope unitOfWorkScope = this._container.BeginLifetimeScope(
        // overriding default connection factory with transactional one
          scopeBuilder => scopeBuilder.RegisterType<TransactionalConnectionFactory>().As<IConnectionFactory>().InstancePerLifetimeScope()
        );

        Debug.Assert(unitOfWorkScope.Resolve<IConnectionFactory>().GetType() == typeof(TransactionalConnectionFactory));
 
        var unitOfWork = new UnitOfWork(unitOfWorkScope);

        return unitOfWork;
    }
}
