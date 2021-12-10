namespace OutboxSample.DomainModel.Events;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class EventMetadataAttribute : Attribute
{
    public string EventType { get; }
    public string AggregateType { get; }
    public uint EventSchemaVersion { get; }

    public EventMetadataAttribute(string eventType, string aggregateType, uint eventSchemaVersion)
    {
        EventType = eventType;
        AggregateType = aggregateType;
        EventSchemaVersion = eventSchemaVersion;
    }
}
