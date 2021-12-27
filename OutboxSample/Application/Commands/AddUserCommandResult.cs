using OutboxSample.Application.DataAccess;

namespace OutboxSample.Application.Commands;

public sealed record AddUserCommandResult(QueryResult DbQueryResult, uint Version);
