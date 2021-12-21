namespace OutboxSample.Application;

public interface IQueryHandler<TQuery, TResult>
{
    TResult Handle(TQuery query);
}
