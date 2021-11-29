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
        // Server=localhot;User Id=st;Password=$5t-m$5q1$;

        SqlConnectionStringBuilder sb = new("Server=localhost;User Id=st;Password=$5t-m$5q1$;")
        { InitialCatalog = "Test" };

        string connectionString = sb.ToString();

        return connectionString;
    }
}