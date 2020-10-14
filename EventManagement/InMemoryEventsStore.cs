using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidCqrsFramework.EventManagement
{
    public class InMemoryEventsStore : IEventsStore
    {
        readonly IEventBus _bus;
        readonly Dictionary<UniverseEvent, List<Event>> _eventsByAggregate;

        public InMemoryEventsStore(IEventBus bus, Dictionary<UniverseEvent, List<Event>> eventsByAggregate)
        {
            this._bus = bus;
            this._eventsByAggregate = eventsByAggregate;
        }

        public async Task Store(AggregateRoot aggreagate)
        {
            var es = aggreagate.UncommittedEvents.ToList();
            var universeEvent = new UniverseEvent {AggrategateId = aggreagate.Id};
            if (_eventsByAggregate.ContainsKey(universeEvent))
                _eventsByAggregate[universeEvent].AddRange(es);
            else
                _eventsByAggregate[universeEvent] = es;

            await _bus.Publish(es);
        }

        public async Task<IEnumerable<Event>> LoadEvents(Guid id, long version = 0)
        {
            var universeEvent = new UniverseEvent { AggrategateId = id };
            if (_eventsByAggregate.ContainsKey(universeEvent) == false)
                return await Task.FromResult(new Event[0]);

            return  _eventsByAggregate[universeEvent];
        }
    }

    public class UniverseEvent : ValueObject<UniverseEvent>
    {
        public Guid AggrategateId { get; set; }
        public Guid RevisionId { get; set; }
        
    }

}
