using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolidCqrsFramework.EventManagement
{
    public interface IEventsStore
    {
        Task Store(AggregateRoot aggregate);
        Task<IEnumerable<Event>> LoadEvents(Guid id, long version = 0);
    }
}
