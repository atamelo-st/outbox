using OutboxSample.Model;

namespace OutboxSample.Application;

public interface IUserRepository : IRepository, ISupportUnitOfWork
{
    QueryResult GetAll();

    User? Get(Guid id);

    bool Add(User user);

    bool AddMany(IEnumerable<User> users);

    bool Delete(Guid id);
}
