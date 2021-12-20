namespace OutboxSample.Application;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> BeginAsync();
}
