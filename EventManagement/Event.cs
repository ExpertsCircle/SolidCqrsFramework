﻿using System;

namespace SolidCqrsFramework.EventManagement
{
    public abstract class Event : IEvent
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
