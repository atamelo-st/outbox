using Autofac;
using Autofac.Extensions.DependencyInjection;
using Npgsql;
using OutboxSample.Application;
using OutboxSample.Infrastructure;
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
        containerBuilder.RegisterType<ConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
        containerBuilder.RegisterType<DefaultConnectionFactory>().As<IConnectionFactory>().SingleInstance();
        // TODO: per-scope?
        containerBuilder.RegisterType<UnitOfWorkFactory>().As<IUnitOfWorkFactory>().SingleInstance();

        containerBuilder.RegisterType<Outbox>().As<IOutbox>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
    }
}
