using OutboxSample.Model;
using static OutboxSample.Application.QueryResult;

namespace OutboxSample.Application;

public interface IUserRepository : IRepository, ISupportUnitOfWork
{
    QueryResult<IEnumerable<DataStoreItem<User>>> GetAll();

    QueryResult<User> Get(Guid id);

    // NOTE: initial version is 0 as we're creating a new object
    QueryResult<int> Add(User user, DateTime createdAt, uint startingVersion = 0);

    QueryResult<int> AddMany(IEnumerable<User> users, DateTime createdAt, uint startingVersion = 0);

    QueryResult<bool> Delete(Guid id);
}
