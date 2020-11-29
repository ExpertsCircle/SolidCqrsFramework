using System;
using System.Threading.Tasks;
using SolidCqrsFramework.EventManagement;

namespace SolidCqrsFramework
{
    public class Repository<T> : IRepository<T> where T : AggregateRoot, new()
    {
        readonly IEventsStore _eventsStore;

        public Repository(IEventsStore eventsStore)
        {
            _eventsStore = eventsStore;
        }

        public async Task Save(AggregateRoot aggregate, int expectedVersion = 0)
        {
            await _eventsStore.Store(aggregate);
        }

        public async Task<T> GetById(string id)
        {
            var events = await _eventsStore.LoadEvents(id);
            var aggregate = (T)Activator.CreateInstance(typeof(T), true);
            aggregate.LoadFromHistory(events);

            return aggregate;
        }
    }
}
