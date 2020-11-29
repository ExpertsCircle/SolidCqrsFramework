using System;

namespace SolidCqrsFramework.EventManagement
{
    public interface IEvent
    {
        string AggregateId { get; }
    }
    public interface IInProcessEvent
    {
    }
}
