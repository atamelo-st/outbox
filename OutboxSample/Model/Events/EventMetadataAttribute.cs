namespace OutboxSample.Model.Events;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class EventMetadataAttribute : Attribute
{
    public EventMetadataAttribute(string eventType, string aggregateType, uint schemaVersion)
    {
    }
}
