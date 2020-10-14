namespace SolidCqrsFramework.Commanding
{
    public interface ICommandHandlerFactory
    {
        ICommandHandler<T> GetHandler<T>() where T : Command;
    }
}
