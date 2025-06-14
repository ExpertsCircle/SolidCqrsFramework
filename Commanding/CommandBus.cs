using System;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.Exceptions;

namespace SolidCqrsFramework.Commanding
{
    public class CommandBus : ICommandBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandBus> _logger;

        public CommandBus(IServiceProvider serviceProvider, ILogger<CommandBus> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Handle<TCommand>(TCommand command) where TCommand : Command
        {
            using (_logger.BeginScope($"Handling Command {command.GetType().Name}", new { CommandName = command.GetType().Name }))
            {
                _logger.LogInformationWithObject("Handling Command details", new
                {
                    CommandName = command.GetType().Name,
                    Message = command
                });

                using var scope = _serviceProvider.CreateScope();
                var scopedProvider = scope.ServiceProvider;

                var handler = GetHandler<TCommand>(scopedProvider);
                if (handler == null)
                    throw new CommandHandlerNotFoundException($"No Command handler found for {command.GetType()}");

                _logger.LogInformationWithObject("Found handler for command", new
                {
                    HandlerName = handler.GetType().Name
                });

                await Validate(command, scopedProvider);

                try
                {
                    await handler.ExecuteAsync(command);
                }
                catch (DomainException ex)
                {
                    _logger.LogWarningWithObject(ex.Message, new
                    {
                        CommandName = command.GetType().Name,
                        HandlerName = handler.GetType().Name,
                        ExceptionType = ex.GetType().Name
                    });
                    throw;
                }
                catch (Exception e)
                {
                    _logger.LogErrorWithObject(e, e.Message, new
                    {
                        CommandName = command.GetType().Name,
                        InnerErrorMessage = e.InnerException?.Message,
                    });
                    throw;
                }

                _logger.LogInformationWithObject("Command handling was successful", new
                {
                    HandlerName = handler.GetType().Name
                });
            }
        }

        private dynamic GetHandler<TCommand>(IServiceProvider scopedProvider) where TCommand : Command
        {
            var commandType = typeof(TCommand);
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
            return scopedProvider.GetService(handlerType);
        }

        private async Task Validate<TCommand>(TCommand command, IServiceProvider scopedProvider) where TCommand : Command
        {
            var commandType = typeof(TCommand);
            var validatorType = typeof(ICommandValidator<>).MakeGenericType(commandType);
            dynamic validator = scopedProvider.GetService(validatorType);

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
