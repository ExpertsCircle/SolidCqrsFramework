using System.Threading.Tasks;

namespace SolidCqrsFramework.Commanding
{
    public interface ICommandValidator<T> where T : Command
    {
        Task Validate(T command);
    }

}
