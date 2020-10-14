using System.Threading.Tasks;

namespace SolidCqrsFramework.Commanding
{
    public interface ICommandHandler<TCommand>
    {
        Task ExecuteAsync(TCommand command);
    }


}
