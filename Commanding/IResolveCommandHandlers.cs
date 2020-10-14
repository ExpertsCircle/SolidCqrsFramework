namespace SolidCqrsFramework.Commanding
{
    public interface IResolveCommandHandlers
    {
        IHandle<TCommand> ResolveHandler<TCommand>(TCommand command) where TCommand : Command;
    }
}