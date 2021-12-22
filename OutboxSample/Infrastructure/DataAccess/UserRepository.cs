using System.Data;
using System.Data.Common;

using OutboxSample.Application.DataAccess;
using OutboxSample.DomainModel;
using OutboxSample.Infrastructure.DataAccess;

namespace OutboxSample.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly IConnectionFactory connectionFactory;

    public UserRepository(IConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory, nameof(connectionFactory));

        this.connectionFactory = connectionFactory;
    }

    public Task<QueryResult<User>> GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<QueryResult<IEnumerable<DataStore.Item<User>>>> GetAllAsync()
    {
        await using DbConnection connection = this.connectionFactory.GetConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM users";
        command.CommandType = CommandType.Text;

        await connection.OpenAsync();

        // TODO: replace with Dapper
        await using DbDataReader dataReader = await command.ExecuteReaderAsync();
        List<DataStore.Item<User>> queryResult = new();

        while (dataReader.Read())
        {
            Guid id = dataReader.GetGuid(dataReader.GetOrdinal("id"));
            string name = dataReader.GetString(dataReader.GetOrdinal("name"));
            User user = new(id, name);

            DateTime createdAt = dataReader.GetDateTime(dataReader.GetOrdinal("created_at"));
            DateTime updatedAt = dataReader.GetDateTime(dataReader.GetOrdinal("updated_at"));
            uint version = (uint)dataReader.GetInt32(dataReader.GetOrdinal("version"));
            DataStore.ItemMetadata metadata = new(version, updatedAt, createdAt);

            queryResult.Add(new(user, metadata));
        }

        return QueryResult.OfSuccess(queryResult.AsEnumerable(), DataStore.ItemMetadata.Empty);
    }

    public async Task<QueryResult<int>> AddAsync(User user, DateTime createdAt, uint startingVersion = 0)
    {
        await using DbConnection connection = this.connectionFactory.GetConnection();
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO users(id, name, created_at, updated_at, version) VALUES(@Id, @Name, @CreatedAt, @UpdatedAt, @Version)";
        command.CommandType = CommandType.Text;
        command.Parameters.Add(command.CreateParameter("@Id", user.Id));
        command.Parameters.Add(command.CreateParameter("@Name", user.Name));
        command.Parameters.Add(command.CreateParameter("@CreatedAt", createdAt));
        command.Parameters.Add(command.CreateParameter("@UpdatedAt", createdAt));
        command.Parameters.Add(command.CreateParameter("@Version", (int) startingVersion));

        await connection.OpenAsync();

        try
        {
            int count = await command.ExecuteNonQueryAsync();

            // NOTE: returning empty metadata as metadata is not read from the db upon adding a user and is known upfront
            return QueryResult.OfSuccess(count, DataStore.ItemMetadata.Empty);
        }
        catch (Exception)
        {
            bool alreadyExists = false; //TODO: analyze exception (+add when condition) to figure out if that's the case

            if (alreadyExists)
            {
                return QueryResult<int>.OfFailure.AlreadyExists();
            }

            throw;
        }
    }

    public async Task<QueryResult<int>> AddManyAsync(IEnumerable<User> users, DateTime createdAt, uint startingVersion = 0)
    {
        await using DbConnection connection = this.connectionFactory.GetConnection();
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        await using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO users(id, name, created_at, updated_at, version) VALUES(@Id, @Name, @CreatedAt, @UpdatedAt, @Version)";
        command.CommandType = CommandType.Text;

        DbParameter idParameter = command.CreateParameter("@Id", DbType.Guid);
        DbParameter nameParamenter = command.CreateParameter("@Name", DbType.String, 50);

        command.Parameters.Add(idParameter);
        command.Parameters.Add(nameParamenter);
        command.Parameters.Add(command.CreateParameter("@CreatedAt", createdAt));
        command.Parameters.Add(command.CreateParameter("@UpdatedAt", createdAt));
        command.Parameters.Add(command.CreateParameter("@Version", (int) startingVersion));

        // TODO: switch to the `unnest` function. Details: https://github.com/npgsql/npgsql/issues/2779#issuecomment-573439342
        // OR the new batching API for Postgres: https://www.roji.org/parameters-batching-and-sql-rewriting
        // OR just SB-base 'batching' as one in the Outbox implemetation
        await command.PrepareAsync();

        int count = 0;

        foreach (User user in users)
        {
            idParameter.Value = user.Id;
            nameParamenter.Value = user.Name;

            count += await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();

        return QueryResult.OfSuccess(count, DataStore.ItemMetadata.Empty);
    }

    public Task<QueryResult<bool>> DeleteAsync(Guid id)
    {
        return Task.FromResult<QueryResult<bool>>(QueryResult<bool>.OfFailure.NotFound()); // :) just a stub
    }

    public async Task<QueryResult> ChangeUserName(User userWithNewName, DateTime updatedAt, uint expectedVersion)
    {
        await using DbConnection connection = this.connectionFactory.GetConnection();
        await using DbCommand command = connection.CreateCommand();

        // TODO: return the old name in the same query ? Not sure we really need it, though..
        // E.g. smth like (https://stackoverflow.com/a/7927957/349658):
        // UPDATE tbl x SET tbl_id = 24, name = 'New Gal'
        // FROM (SELECT tbl_id, name FROM tbl WHERE tbl_id = 4 FOR UPDATE) y
        // WHERE x.tbl_id = y.tbl_id
        // RETURNING y.tbl_id AS old_id, y.name AS old_name, x.tbl_i, x.name;
        command.CommandText = "UPDATE users SET name=@NewName, updated_at=@UpdatedAt, version=version+1 WHERE id=@Id AND version=@ExpectedVersion";
        command.CommandType = CommandType.Text;
        command.Parameters.Add(command.CreateParameter("@Id", userWithNewName.Id));
        command.Parameters.Add(command.CreateParameter("@NewName", userWithNewName.Name));
        command.Parameters.Add(command.CreateParameter("@UpdatedAt", updatedAt));
        command.Parameters.Add(command.CreateParameter("@ExpectedVersion", (int)expectedVersion));

        await connection.OpenAsync();

        bool versionMismatch = await command.ExecuteNonQueryAsync() == 0;

        if (versionMismatch)
        {
            return QueryResult<Common.Void>.OfFailure.ConcurrencyConflict();
        }

        // NOTE: if we got here, the UPDATE has completed with the expectedVersion matching the actual one.
        // So, we know that the next version is going to be expectedVersion + 1 (see the UPDATE statement)
        uint newVersion = expectedVersion + 1;
        DataStore.ItemMetadata metadata = new(newVersion, updatedAt);

        return QueryResult.OfSuccess(metadata);
    }
}
