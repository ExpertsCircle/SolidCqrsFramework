using System.Threading.Tasks;

namespace SolidCqrsFramework.Commanding
{
    public interface ICommandBus
    {
        Task Handle<TCommand>(TCommand command) where TCommand : Command;
        // void HandleAsync<TCommand>(TCommand command) where TCommand : Command;
    }
}
