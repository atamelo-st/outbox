using OutboxSample.Model;

namespace OutboxSample.Application;

public interface IUserRepository : IRepository, ISupportUnitOfWork
{
    QueryResult<IEnumerable<User>> GetAll();

    QueryResult<User> Get(Guid id);

    QueryResult<int> Add(User user);

    QueryResult<int> AddMany(IEnumerable<User> users);

    QueryResult<bool> Delete(Guid id);
}
