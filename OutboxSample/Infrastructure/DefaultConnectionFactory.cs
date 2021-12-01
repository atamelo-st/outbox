﻿using Npgsql;
using System.Data;
using System.Data.SqlClient;

namespace OutboxSample.Infrastructure;

public class DefaultConnectionFactory : IConnectionFactory
{
    private readonly IConnectionStringProvider connectionStringProvider;

    public DefaultConnectionFactory(IConnectionStringProvider connectionStringProvider)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider, nameof(connectionStringProvider));

        this.connectionStringProvider = connectionStringProvider;
    }

    public IDbConnection GetConnection()
    {
        string connectionString = this.connectionStringProvider.GetConnectionString();

        // var connection = new SqlConnection(connectionString);
        var connection = new NpgsqlConnection(connectionString);

        return connection;
    }
}
