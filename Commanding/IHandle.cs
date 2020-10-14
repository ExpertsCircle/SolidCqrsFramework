namespace SolidCqrsFramework.Commanding
{
    public interface IHandle<in TCommand> where TCommand : IAmACommand
    {
        void Execute(TCommand command);
    }
}
