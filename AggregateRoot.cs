using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SolidCqrsFramework.EventManagement;

namespace SolidCqrsFramework
{
    public abstract class AggregateRoot
    {
        public long Version { get; private set; }
        protected AggregateRoot(Guid id)
        {
            Id = id;
            _uncommittedEvents = new List<Event>();
        }

        protected AggregateRoot()
        {
            Id = Guid.NewGuid();
            _uncommittedEvents = new List<Event>();
        }

        public Guid Id { get; set; }

        #region Event storage

        private readonly List<Event> _uncommittedEvents;

        public ReadOnlyCollection<Event> UncommittedEvents { get { return _uncommittedEvents.AsReadOnly(); } }

        public void MarkClean()
        {
            _uncommittedEvents.Clear();
        }
        
        public void LoadFromHistory(IEnumerable<Event> events)
        {
            foreach (Event @event in events)
            {
                HandleEvent(@event, false);
            }
        }

        #endregion

        public void HandleEvent(Event @event, bool isNew = true)
        {
            dynamic thisAsDynamic = this;
            var e = Cast(@event, @event.GetType());
            thisAsDynamic.Handle(e);

            if (isNew)
                _uncommittedEvents.Add(@event);
            
            Version++;
        }

        public static dynamic Cast(dynamic obj, Type castTo)
        {
            return Convert.ChangeType(obj, castTo);
        }
    }
}
