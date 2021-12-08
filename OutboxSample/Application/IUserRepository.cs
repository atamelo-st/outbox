using OutboxSample.Model;

namespace OutboxSample.Application;

// TODO: introduce a concept of `RepositoryResponse`/`RepositoryResult`
public interface IUserRepository : IRepository, ISupportUnitOfWork
{
    IEnumerable<User> GetAll();

    User? Get(Guid id);

    bool Add(User user);

    bool AddMany(IEnumerable<User> users);

    bool Delete(Guid id);
}
