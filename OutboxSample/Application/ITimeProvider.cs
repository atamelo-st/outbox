namespace OutboxSample.Application;

public interface ITimeProvider
{
    DateTime Now => DateTime.Now;
}

public class DefaultTimeProvider : ITimeProvider {}
