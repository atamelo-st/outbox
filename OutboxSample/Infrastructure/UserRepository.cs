using OutboxSample.Application.DataAccess;
using OutboxSample.Model;
using System.Data;
using System.Data.Common;

namespace OutboxSample.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly IConnectionFactory connectionFactory;

    public UserRepository(IConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public QueryResult<int> Add(User user, DateTime createdAt, uint startingVersion = 0)
    {
        using (IDbConnection connection = this.connectionFactory.GetConnection())
        using (IDbCommand command = connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO users(id, name, created_at, updated_at, version) VALUES(@Id, @Name, @CreatedAt, @UpdatedAt, @Version)";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(command.CreateParameter("@Id", user.Id));
            command.Parameters.Add(command.CreateParameter("@Name", user.Name));
            command.Parameters.Add(command.CreateParameter("@CreatedAt", createdAt));
            command.Parameters.Add(command.CreateParameter("@UpdatedAt", createdAt));
            command.Parameters.Add(command.CreateParameter("@Version", startingVersion));

            connection.Open();

            try
            {
                int count = command.ExecuteNonQuery();

                // NOTE: returning empty metadata is metadata is not read from the db upon adding a user and nown upfront
                return QueryResult.OfSuccess(count, DataStore.ItemMetadata.Empty);
            }
            catch (Exception)
            {
                bool alreadyExists = false; //TODO: analyze exception to figure out if that's the case

                if (alreadyExists)
                {
                    return QueryResult<int>.OfFailure.AlreadyExists();
                }

                throw;
            }
        }
    }

    public QueryResult<int> AddMany(IEnumerable<User> users, DateTime createdAt, uint startingVersion = 0)
    {
        using IDbConnection connection = this.connectionFactory.GetConnection();
        connection.Open();
        using IDbTransaction transaction = connection.BeginTransaction();

        using IDbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO users(id, name, created_at, updated_at, version) VALUES(@Id, @Name, @CreatedAt, @UpdatedAt, @Version)";
        command.CommandType = CommandType.Text;

        DbParameter idParameter = command.CreateParameter("@Id", DbType.Guid);
        DbParameter nameParamenter = command.CreateParameter("@Name", DbType.String, 50);

        command.Parameters.Add(idParameter);
        command.Parameters.Add(nameParamenter);
        command.Parameters.Add(command.CreateParameter("@CreatedAt", createdAt));
        command.Parameters.Add(command.CreateParameter("@UpdatedAt", createdAt));
        command.Parameters.Add(command.CreateParameter("@Version", startingVersion));

        // TODO: switch to the `unnest` function. Details: https://github.com/npgsql/npgsql/issues/2779#issuecomment-573439342
        // OR the new batching API for Postgres: https://www.roji.org/parameters-batching-and-sql-rewriting
        // OR just SB-base 'batching' as one in the Outbox implemetation
        command.Prepare();

        int count = 0;

        foreach (User user in users)
        {
            idParameter.Value = user.Id;
            nameParamenter.Value = user.Name;

            count += command.ExecuteNonQuery();
        }

        transaction.Commit();

        return QueryResult.OfSuccess(count, DataStore.ItemMetadata.Empty);
    }

    public QueryResult<bool> Delete(Guid id)
    {
        return QueryResult<bool>.OfFailure.NotFound(); // :) just a stub
    }

    public QueryResult<User> Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public QueryResult<IEnumerable<DataStore.Item<User>>> GetAll()
    {
        using (IDbConnection connection = this.connectionFactory.GetConnection())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM users";
            command.CommandType = CommandType.Text;

            connection.Open();

            // TODO: replace with Dapper
            using (IDataReader dataReader = command.ExecuteReader())
            {
                List<DataStore.Item<User>> queryResult = new();

                while (dataReader.Read())
                {
                    Guid id = dataReader.GetGuid(dataReader.GetOrdinal("id"));
                    string name = dataReader.GetString(dataReader.GetOrdinal("name"));
                    User user = new(id, name);

                    DateTime createdAt = dataReader.GetDateTime(dataReader.GetOrdinal("created_at"));
                    DateTime updatedAt = dataReader.GetDateTime(dataReader.GetOrdinal("updated_at"));
                    uint version = (uint)dataReader.GetInt32(dataReader.GetOrdinal("version"));
                    DataStore.ItemMetadata metadata = new(createdAt, updatedAt, version);

                    queryResult.Add(new(user, metadata));
                }

                return QueryResult.OfSuccess(queryResult.AsEnumerable(), DataStore.ItemMetadata.Empty);
            }
        }
    }
}
