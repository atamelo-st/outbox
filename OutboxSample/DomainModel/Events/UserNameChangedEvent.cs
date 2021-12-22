namespace OutboxSample.DomainModel.Events;

[EventMetadata("user-name-changed", "application-aggregate", 0)]
public sealed record UserNameChangedEvent(Guid Id, Guid UserId, string UserNewName) : IEvent;