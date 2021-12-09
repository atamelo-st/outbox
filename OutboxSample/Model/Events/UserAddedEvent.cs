namespace OutboxSample.Model.Events;

[EventMetadata("user-added", "application-aggregate", 0)]
public readonly record struct UserAddedEvent : IEvent
{
    public Guid UserId { get; }
    public string UserName { get; }

    public Guid Id { get; }

    public UserAddedEvent(Guid Id, Guid userId, string userName)
    {
        ArgumentNullException.ThrowIfNull(userName, nameof(userName));

        this.Id = Id;
        UserId = userId;
        UserName = userName;
    }

    public UserAddedEvent()
    {
        throw new NotSupportedException("Must use constructor with parameters.");
    }
}
