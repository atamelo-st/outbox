using Npgsql;
using System.Data.SqlClient;

namespace OutboxSample.Infrastructure.DataAccess;

public interface IConnectionStringProvider
{
    string GetConnectionString(string? databaseName = null);
}

public class ConnectionStringProvider : IConnectionStringProvider
{
    public string GetConnectionString(string? databaseName = null)
    {
        //SqlConnectionStringBuilder sb = new("Server=localhost;User Id=st;Password=$5t-m$5q1$;")
        //{ InitialCatalog = "Test" };

        NpgsqlConnectionStringBuilder sb = new("Host=localhost;Username=admin;Password=admin;Database=testdatabase;");

        if (databaseName is not null)
        {
            sb.Database = databaseName;
        }

        sb.Port = 5499;

        string connectionString = sb.ToString();

        return connectionString;
    }
}