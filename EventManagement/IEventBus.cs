using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolidCqrsFramework.EventManagement
{
    public interface IEventBus
    {
        Task Publish<T>(IEnumerable<T> events) where T : Event;
    }
}
