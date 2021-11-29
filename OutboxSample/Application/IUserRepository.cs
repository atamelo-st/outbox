using OutboxSample.Model;

namespace OutboxSample.Application;

public interface IUserRepository : IRepository, ISupportUnitOfWork
{
    IEnumerable<User> GetAll();

    User? Get(Guid id);

    bool Add(User user);

    bool Delete(Guid id);
}
