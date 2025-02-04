using System.Threading.Tasks;

namespace SolidCqrsFramework.Commanding
{
    public interface ICommandValidator<in T> where T : Command
    {
        Task Validate(T command);
    }

}
