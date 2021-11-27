namespace OutboxSample.Application;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Begin();
}
