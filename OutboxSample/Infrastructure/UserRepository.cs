using OutboxSample.Application;
using OutboxSample.Model;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace OutboxSample.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly IConnectionFactory connectionFactory;

    public UserRepository(IConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public bool Add(User user)
    {
        using (IDbConnection connection = this.connectionFactory.GetConnection())
        using (IDbCommand command = connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO users VALUES(@pID, @pName)";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(command.CreateParameter("@pID", user.Id));
            command.Parameters.Add(command.CreateParameter("@pName", user.Name));

            connection.Open();

            int count = command.ExecuteNonQuery();

            return count > 0;
        }
    }

    public bool AddMany(IEnumerable<User> users)
    {
        int count = 0;

        using IDbConnection connection = this.connectionFactory.GetConnection();
        connection.Open();
        using IDbTransaction transaction = connection.BeginTransaction();

        foreach (User user in users)
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO users VALUES(@pID, @pName)";
                command.CommandType = CommandType.Text;
                command.Parameters.Add(command.CreateParameter("@pID", user.Id));
                command.Parameters.Add(command.CreateParameter("@pName", user.Name));
                command.Transaction = transaction;

                count += command.ExecuteNonQuery();
            }

        transaction.Commit();

        return count > 0;
    }

    public bool Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public User? Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<User> GetAll()
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

                return users;
            }
        }
    }
}
