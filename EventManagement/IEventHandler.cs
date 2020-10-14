using System.Threading.Tasks;

namespace SolidCqrsFramework.EventManagement
{
    public interface IEventHandler<TEvent> where TEvent : Event
    {
        Task Handle(TEvent handle);
    }
}
