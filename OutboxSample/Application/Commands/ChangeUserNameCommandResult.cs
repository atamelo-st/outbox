using OutboxSample.Application.DataAccess;

namespace OutboxSample.Application.Commands;

public sealed record ChangeUserNameCommandResult(QueryResult DbQueryResult);
