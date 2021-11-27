using OutboxSample.Application;
using OutboxSample.Model;
using System.Data;

namespace OutboxSample.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public UserRepository(IConnectionFactory connectionFactory)
        {
            this._connectionFactory = connectionFactory;
        }

        public bool Add(User user)
        {
            using (IDbConnection connection = this._connectionFactory.GetConnection())
            using (IDbCommand command = connection.CreateCommand())
            {
                // open connection
                // execute comand
                // close connection

                command.CommandText = "SQL goes here";
                command.CommandType = CommandType.Text;
                // command.Parameters = ...
            }

            throw new NotImplementedException();
        }

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public User? Get(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
