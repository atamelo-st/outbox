namespace OutboxSample.Application.Commands;

public sealed record AddUserCommand(Guid UserId, string UserName);
