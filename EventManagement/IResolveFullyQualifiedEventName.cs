namespace SolidCqrsFramework.EventManagement
{
    public interface IResolveFullyQualifiedEventName
    {
        string this[string eventName] { get; }
    }
}