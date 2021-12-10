using OutboxSample.Application.DataAccess;
using OutboxSample.Application.Eventing;

namespace OutboxSample.Application;

public interface IUnitOfWork : IDisposable
{
    bool Commit();
    void Rollback();

    TRepository GetRepository<TRepository>() where TRepository : IRepository, ISupportUnitOfWork;

    IOutbox GetOutbox();
}

