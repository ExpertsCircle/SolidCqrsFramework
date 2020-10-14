using System.Collections.Generic;

namespace SolidCqrsFramework.EventManagement
{
    public interface IEventHandlerFactory
    {
        IEnumerable<IEventHandler<T>> GetHandlers<T>() where T : Event;
    }
}