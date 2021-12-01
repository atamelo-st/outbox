using Autofac;
using Autofac.Extensions.DependencyInjection;
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

        RunStartupTasks(app.Services);

        app.Run();
    }

    static void RegisterDependencies(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<ConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
        containerBuilder.RegisterType<DefaultConnectionFactory>().As<IConnectionFactory>().SingleInstance();
        // TODO: per-scope?
        containerBuilder.RegisterType<UnitOfWorkFactory>().As<IUnitOfWorkFactory>().SingleInstance();

        containerBuilder.RegisterType<Outbox>().As<IOutbox>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
    }

    private static void RunStartupTasks(IServiceProvider services)
    {
        var connectionFactory = services.GetRequiredService<IConnectionFactory>();

        using (IDbConnection connection = connectionFactory.GetConnection())
        using (IDbCommand command = connection.CreateCommand())
        {
            command.CommandText =
@"
CREATE TABLE IF NOT EXISTS public.outbox
(
    EventId uuid NOT NULL,
    Payload character varying COLLATE pg_catalog.""default"" NOT NULL,
    CONSTRAINT ""Outbox_pkey"" PRIMARY KEY(EventId)
);

ALTER TABLE IF EXISTS public.outbox OWNER to postgres;

CREATE TABLE IF NOT EXISTS public.users
(
    Id uuid NOT NULL,
    Name character varying COLLATE pg_catalog.""default"" NOT NULL,
    CONSTRAINT ""Users_pkey"" PRIMARY KEY(Id)
);

ALTER TABLE IF EXISTS public.users OWNER to postgres;
";
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
