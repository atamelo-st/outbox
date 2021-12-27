namespace OutboxSample.Common;

public interface ITimeProvider
{
    DateTime UtcNow => DateTime.UtcNow;
}

public class DefaultTimeProvider : ITimeProvider { }
