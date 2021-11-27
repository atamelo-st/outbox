namespace OutboxSample.Application;

public interface IUnitOfWork : IDisposable
{
    void Commit();
    void Rollback();

    TRepository GetRepository<TRepository>() where TRepository : IRepository, ISupportUnitOfWork;

    IOutbox GetOutbox();
}

