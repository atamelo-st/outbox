namespace OutboxSample.Common;

public sealed record Void
{
    public static readonly Void Instance = new Void();
    private Void()
    {
    }
}

