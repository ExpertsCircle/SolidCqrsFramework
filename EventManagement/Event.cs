using System;

namespace SolidCqrsFramework.EventManagement
{
    public abstract class Event : IEvent
    {
        public Guid AggregateId { get; private set; }
        public DateTime Date { get; private set; }
        protected Event(Guid aggregateId, DateTime date)
        {
            AggregateId = aggregateId;
            Date = date;
        }
    }
}
