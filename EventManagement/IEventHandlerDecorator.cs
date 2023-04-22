using System.Threading.Tasks;

namespace SolidCqrsFramework.EventManagement
{
    public interface IEventHandlerDecorator<TEvent> : IEventHandler<TEvent> where TEvent : Event
    {
        Task HandleDecorated(TEvent @event);
    }
}
