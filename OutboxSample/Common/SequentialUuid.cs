using SequentialGuid;

namespace OutboxSample.Common;

public static class SequentialUuid
{
    public static Guid New() => SequentialGuidGenerator.Instance.NewGuid();
}
