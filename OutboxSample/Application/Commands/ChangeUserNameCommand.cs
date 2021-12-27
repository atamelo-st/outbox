namespace OutboxSample.Application.Commands;

public sealed record ChangeUserNameCommand(Guid userId, string newName, uint expectedVersion);
