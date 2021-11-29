using Autofac;
using Autofac.Extensions.DependencyInjection;
using OutboxSample.Application;
using OutboxSample.Infrastructure;
using OutboxSample.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(RegisterDependencies);

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();


static void RegisterDependencies(ContainerBuilder containerBuilder)
{
    containerBuilder.RegisterType<ConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
    containerBuilder.RegisterType<DefaultConnectionFactory>().As<IConnectionFactory>().SingleInstance();
    // TODO: per-scope?
    containerBuilder.RegisterType<UnitOfWorkFactory>().As<IUnitOfWorkFactory>().SingleInstance();

    containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
}

