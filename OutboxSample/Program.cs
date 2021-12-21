using Autofac;
using Autofac.Extensions.DependencyInjection;
using Npgsql;
using OutboxSample.Application;
using OutboxSample.Application.Commands;
using OutboxSample.Application.DataAccess;
using OutboxSample.Application.Eventing;
using OutboxSample.Application.Queries;
using OutboxSample.Application.QueryHandlers;
using OutboxSample.Common;
using OutboxSample.DomainModel;
using OutboxSample.Infrastructure;
using OutboxSample.Infrastructure.DataAccess;
using OutboxSample.Infrastructure.Eventing;
using System.Data;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.Host.ConfigureContainer<ContainerBuilder>(RegisterDependencies);
        builder.Services.AddControllers();

        WebApplication app = builder.Build();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    static void RegisterDependencies(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<DefaultTimeProvider>().As<ITimeProvider>().SingleInstance();
        containerBuilder.RegisterType<AttributeSourcedEventMetadataProvider>().As<IEventMetadataProvider>().SingleInstance();
        containerBuilder.RegisterType<ConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
        containerBuilder.RegisterType<DefaultConnectionFactory>().As<IConnectionFactory>().SingleInstance();
        // TODO: per-scope?
        containerBuilder.RegisterType<UnitOfWorkFactory>().As<IUnitOfWorkFactory>().SingleInstance();

        containerBuilder.RegisterType<Outbox>().As<IOutbox>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

        containerBuilder.RegisterType<UserCommandQueryHandler>().As<IQueryHandler<GetUserQuery, QueryResult<User>>>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<UserCommandQueryHandler>().As<IQueryHandler<GetAllUsersQuery, QueryResult<IEnumerable<DataStore.Item<User>>>>>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<UserCommandQueryHandler>().As<ICommandHandler<AddUserCommand, AddUserCommandResult>>().InstancePerLifetimeScope();
    }
}
