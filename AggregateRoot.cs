using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using SolidCqrsFramework.EventManagement;

namespace SolidCqrsFramework
{
    public abstract class AggregateRoot
    {
        public long Version { get; private set; }
        protected AggregateRoot(string id)
        {
            Id = id;
            _uncommittedEvents = new List<Event>();
        }

        protected AggregateRoot()
        {
            Id = Guid.NewGuid().ToString();
            _uncommittedEvents = new List<Event>();
        }

        public string Id { get; set; }

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

        //public void HandleEvent(Event @event, bool isNew = true)
        //{
        //    dynamic thisAsDynamic = this;
        //    var e = Cast(@event, @event.GetType());
        //    thisAsDynamic.Handle(e);

        //    if (isNew)
        //        _uncommittedEvents.Add(@event);
            
        //    Version++;
        //}

        public void HandleEvent(Event @event, bool isNew = true)
        {
            MethodInfo method = this.GetType().GetMethod("UpdateFrom", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { @event.GetType() }, null);
            if (method != null)
            {
                method.Invoke(this, new object[] { @event });
            }

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
