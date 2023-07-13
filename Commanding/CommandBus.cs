using System;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.Exceptions;

namespace SolidCqrsFramework.Commanding
{
    public class CommandBus : ICommandBus
    {
        private readonly IServiceProvider _container;
        private readonly ILogger<CommandBus> _logger;
        public CommandBus(IServiceProvider container, ILogger<CommandBus> logger)
        {
            _container = container;
            _logger = logger;
        }

        public async Task Handle<TCommand>(TCommand command) where TCommand : Command
        {
            using (_logger.BeginScope($"Handing Command {command.GetType().Name}", new { CommandName = command.GetType().Name }))
            {
                _logger.LogInformationWithObject("Command details", new
                {
                    CommandName = command.GetType().Name,
                    Message = command
                });

                var com = command.GetType();
                var handlerType = typeof(ICommandHandler<>).MakeGenericType(com);
                dynamic handler = _container.GetService(handlerType);




                if (handler == null)
                    throw new CommandHandlerNotFoundException($"No Command handler found for {command.GetType()}");

                _logger.LogInformationWithObject("Found handler for command", new
                {
                    HandlerName = handler.GetType().Name
                });

                await Validate(command);

                await handler.ExecuteAsync(command);

                _logger.LogInformationWithObject("Command handing was successful", new
                {
                    HandlerName = handler.GetType().Name
                });
            }
        }

        async Task Validate<TCommand>(TCommand command) where TCommand : Command
        {
            var com = command.GetType();
            var handlerType = typeof(ICommandValidator<>).MakeGenericType(com);

            dynamic validator = _container.GetService(handlerType);

            if (validator != null)
            {
                _logger.LogInformationWithObject("Found validator for command", new
                {
                    ValidatorName = validator.GetType().Name
                });

                await validator.Validate(command);
            }
        }
        }


}
