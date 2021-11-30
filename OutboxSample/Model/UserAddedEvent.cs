namespace OutboxSample.Model;

public readonly record struct UserAddedEvent
{
    public Guid UserId { get; }
    public string UserName { get; }

    public UserAddedEvent(Guid userId, string userName)
    {
        ArgumentNullException.ThrowIfNull(userName, nameof(userName));

        UserId = userId;
        UserName = userName;
    }
}
