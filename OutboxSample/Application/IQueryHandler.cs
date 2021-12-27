namespace OutboxSample.Application;

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}

public delegate Task<TResult> QueryHandler<TQuery, TResult>(TQuery query);