using Npgsql;
using System.Data.SqlClient;

namespace OutboxSample.Infrastructure;

public interface IConnectionStringProvider
{
    string GetConnectionString();
}

public class ConnectionStringProvider : IConnectionStringProvider
{
    public string GetConnectionString()
    {
        //SqlConnectionStringBuilder sb = new("Server=localhost;User Id=st;Password=$5t-m$5q1$;")
        //{ InitialCatalog = "Test" };

        NpgsqlConnectionStringBuilder sb = new("Host=localhost;Username=postgres;Password=changeme;Database=TestDatabase;");
        sb.Port = 5432;
        // sb.SearchPath = "public";

        string connectionString = sb.ToString();

        return connectionString;
    }
}