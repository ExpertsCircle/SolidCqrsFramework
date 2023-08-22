using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.Aws;

namespace SolidCqrsFramework.EventManagement;

public class EventProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly bool _isInMemory;
    private readonly ILogger<EventProcessor> _logger;
    private readonly IDictionary<Type, dynamic> _decoratedHandlers;

    public EventProcessor(IServiceProvider serviceProvider, bool isInMemory)
    {
        _serviceProvider = serviceProvider;
        _isInMemory = isInMemory;
        _logger = serviceProvider.GetRequiredService<ILogger<EventProcessor>>();
        _decoratedHandlers = new Dictionary<Type, dynamic>();
    }

    public async Task ProcessEventAsync(Type eventType, dynamic message)
    {
        _logger.LogInformationWithObject("Processing Event in EventProcessor", new
        {
            EventName = eventType,
            Message = message
        });

        var decoratedHandlers = GetDecoratedHandlers(eventType);


        if (decoratedHandlers.Count == 0)
        {
            _logger.LogInformationWithObject("No decorator handlers found for event",
                new
                {
                    EventName = eventType.Name
                });
        }


        foreach (var handler in decoratedHandlers)
        {
            try
            {
                await handler.Handle(message);
                _logger.LogInformationWithObject("Successfully handled event",
                    new
                    {
                        EventName = eventType.Name,
                        HandlerName = handler.GetType().Name,
                    });
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithObject(ex ,"Failed to handle event",
                    new
                    {
                        EventName = eventType.Name,
                        HandlerName = handler.GetType().Name,
                        ExceptionDetails = ex.Message
                    });
            }
            
        }
    }

    private dynamic GetDecoratedHandlers(Type eventType)
    {
        if (_decoratedHandlers.ContainsKey(eventType))
            return _decoratedHandlers[eventType];

        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        var decoratorType = typeof(CloudWatchMetricEventHandlerDecorator<>).MakeGenericType(eventType);

        var metricRecorder = _serviceProvider.GetService<CloudWatchMetricRecorder>();
        var loggerType = typeof(ILogger<>).MakeGenericType(decoratorType);

        var logger = _serviceProvider.GetRequiredService(loggerType);

        var decoratedHandlers = handlers.Select(handler =>
            Activator.CreateInstance(decoratorType, handler, metricRecorder, logger, _isInMemory)).ToList();

        _decoratedHandlers.Add(eventType, decoratedHandlers);

        _logger.LogInformationWithObject("Found decorated handlers for event", new
        {
            EventName = eventType,
            DecoratedHandlers = decoratedHandlers.Select(x => x.GetType().Name)
        });

        return decoratedHandlers;
    }
}

