using System;

namespace SolidCqrsFramework.EventManagement
{
    public interface IEvent
    {
        Guid AggregateId { get; }
    }
    public interface IInProcessEvent
    {
    }
}
