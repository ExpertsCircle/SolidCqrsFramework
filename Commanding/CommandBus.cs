using System;
using System.Threading.Tasks;
using SolidCqrsFramework.Exceptions;

namespace SolidCqrsFramework.Commanding
{
    public class CommandBus : ICommandBus
    {
        private readonly IServiceProvider _container;

        public CommandBus(IServiceProvider container)
        {
            _container = container;
        }

        public async Task Handle<TCommand>(TCommand command) where TCommand : Command
        {
            var com = command.GetType();
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(com);

            dynamic handler = _container.GetService(handlerType);
            if (handler == null)
                throw new CommandHandlerNotFoundException($"No Command handler found for {nameof(com)}");

            await Validate(command);
            await handler.ExecuteAsync(command);
        }

        async Task Validate<TCommand>(TCommand command) where TCommand : Command
        {
            var com = command.GetType();
            var handlerType = typeof(ICommandValidator<>).MakeGenericType(com);

            dynamic validator = _container.GetService(handlerType);

            if (validator != null)
                await validator.Validate(command);
        }
    }


}
