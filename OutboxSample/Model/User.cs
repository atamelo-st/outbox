namespace OutboxSample.Model;

public record User
{
    public Guid Id { get; }
    public string Name { get; }

    public User(Guid id, string name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        Id = id;
        Name = name;
    }
}
