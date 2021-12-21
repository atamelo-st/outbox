using OutboxSample.DomainModel;

namespace OutboxSample.Application.DataAccess
{
    public interface IUserRepository : IRepository, ISupportUnitOfWork
    {
        // TODO: IEnumerable<> might not be descriptive enough in some cases. E.g. hen you return a paginated result.
        // Think of a deicated 'slice' type. Smth like PageResult<T> {ItemCount: int, ItemsTotal: int, Items:T[] }
        Task<QueryResult<IEnumerable<DataStore.Item<User>>>> GetAllAsync();

        Task<QueryResult<User>> GetAsync(Guid id);

        // NOTE: initial version is 0 as we're creating a new object
        // TODO: should we just return Void/Unit instead?
        Task<QueryResult<int>> AddAsync(User user, DateTime createdAt, uint startingVersion = 0);

        Task<QueryResult<int>> AddManyAsync(IEnumerable<User> users, DateTime createdAt, uint startingVersion = 0);

        Task<QueryResult<bool>> DeleteAsync(Guid id);

        // TODO: add 'update' API
    }
}