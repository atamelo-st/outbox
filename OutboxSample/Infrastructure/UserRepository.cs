using OutboxSample.Application;
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

    public QueryResult<int> Add(User user)
    {
        using (IDbConnection connection = this.connectionFactory.GetConnection())
        using (IDbCommand command = connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO users VALUES(@pID, @pName)";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(command.CreateParameter("@pID", user.Id));
            command.Parameters.Add(command.CreateParameter("@pName", user.Name));

            connection.Open();

            try
            {
                int count = command.ExecuteNonQuery();

                return QueryResult.OfSuccess(count);
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

    public QueryResult<int> AddMany(IEnumerable<User> users)
    {
        int count = 0;

        using IDbConnection connection = this.connectionFactory.GetConnection();
        connection.Open();
        using IDbTransaction transaction = connection.BeginTransaction();

        using IDbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO users VALUES(@pID, @pName)";
        command.CommandType = CommandType.Text;

        DbParameter idParameter = command.CreateParameter("@pID", DbType.Guid);
        DbParameter nameParamenter = command.CreateParameter("@pName", DbType.String, 50);

        command.Parameters.Add(idParameter);
        command.Parameters.Add(nameParamenter);
        // TODO: switch to the `unnest` function. Details: https://github.com/npgsql/npgsql/issues/2779#issuecomment-573439342
        // OR the new batching API for Postgres: https://www.roji.org/parameters-batching-and-sql-rewriting
        command.Prepare();

        foreach (User user in users)
        {
            idParameter.Value = user.Id;
            nameParamenter.Value = user.Name;

            count += command.ExecuteNonQuery();
        }

        transaction.Commit();

        return QueryResult.OfSuccess(count);
    }

    public QueryResult<bool> Delete(Guid id)
    {
        return QueryResult<bool>.OfFailure.NotFound(); // :) just a stub
    }

    public QueryResult<User> Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public QueryResult<IEnumerable<User>> GetAll()
    {
        using (IDbConnection connection = this.connectionFactory.GetConnection())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM users";
            command.CommandType = CommandType.Text;

            connection.Open();

            using (IDataReader dataReader = command.ExecuteReader())
            {
                List<User> users = new();

                while (dataReader.Read())
                {
                    Guid id = dataReader.GetGuid(dataReader.GetOrdinal("id"));
                    string name = dataReader.GetString(dataReader.GetOrdinal("name"));

                    users.Add(new User(id, name));
                }

                return QueryResult.OfSuccess(users.AsEnumerable());
            }
        }
    }
}
