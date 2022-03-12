using System;
using JustSaying.Models;

namespace SolidCqrsFramework.EventManagement
{
    public abstract class Event : Message, IEvent
    {
        public string AggregateId { get;}
        public DateTime Date { get;}
        protected Event(string aggregateId, DateTime date)
        {
            AggregateId = aggregateId;
            Date = date;
        }
    }
}
