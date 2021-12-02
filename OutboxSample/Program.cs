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

        using (IDbConnection connection = connectionFactory.GetConnection("postgres"))
        using (IDbCommand command = connection.CreateCommand())
        {
            connection.Open();

            // TODO: move it all to a startup postgre script
            command.CommandText = "SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('testdatabase');";

            using (IDataReader reader = command.ExecuteReader())
            {
                bool dbExists = reader.Read();

                if (dbExists is not true)
                {
                    reader.Close();
                    command.CommandText =
    @"
CREATE DATABASE testdatabase
    WITH
    OWNER = admin
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;
";

                    command.ExecuteNonQuery();
                }
            }

            connection.ChangeDatabase("testdatabase");

            command.CommandText =
@"CREATE TABLE IF NOT EXISTS public.outbox_events
(
    id uuid NOT NULL,
    aggregate_type text COLLATE pg_catalog.""default"",
    aggregate_id uuid NOT NULL,
    type text COLLATE pg_catalog.""default"",
    payload text COLLATE pg_catalog.""default"",
    CONSTRAINT pk_outbox_events PRIMARY KEY(id)
);

ALTER TABLE IF EXISTS public.outbox_events OWNER to admin;

CREATE TABLE IF NOT EXISTS public.users
(
    id uuid NOT NULL,
    name character varying COLLATE pg_catalog.""default"" NOT NULL,
    CONSTRAINT pk_users PRIMARY KEY(id)
);

ALTER TABLE IF EXISTS public.users OWNER to admin;
";
            command.ExecuteNonQuery();
        }
    }
}
