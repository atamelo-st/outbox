namespace OutboxSample.Application;

public interface ICommandHandler<TCommand, TResult>
{
    TResult Handle(TCommand command);
}
