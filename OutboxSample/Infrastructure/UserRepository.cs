using OutboxSample.Application;
using OutboxSample.Model;
using System.Data;
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
        using (var command = connection.CreateCommand<SqlCommand>())
        {
            command.CommandText = "SQL goes here";
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(new SqlParameter[0]);

            int count = command.ExecuteNonQuery();

            return count > 0;
        }
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
        using (var command = connection.CreateCommand<SqlCommand>())
        {
            command.CommandText = "SELECT * FROM users";
            command.CommandType = CommandType.Text;
            // command.Parameters.AddRange(new SqlParameter[0]);

            connection.Open();

            using (SqlDataReader dataReader = command.ExecuteReader())
            {
                List<User> users = new();

                while (dataReader.Read())
                {
                    Guid id = dataReader.GetGuid("id");
                    string name = dataReader.GetString("name");

                    users.Add(new User(id, name));
                }

                return users;
            }
        }
    }
}
