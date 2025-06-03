using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.EventManagement;
using SolidCqrsFramework.Exceptions;

namespace SolidCqrsFramework.Aws
{
    public class CloudWatchMetricEventHandlerDecorator<TEvent> : IEventHandlerDecorator<TEvent> where TEvent : Event
    {
        private readonly IEventHandler<TEvent> _innerHandler;
        private readonly CloudWatchMetricRecorder _metricRecorder;
        private readonly ILogger<CloudWatchMetricEventHandlerDecorator<TEvent>> _logger;
        private readonly bool _isInMemoryHandler;

        public CloudWatchMetricEventHandlerDecorator(IEventHandler<TEvent> innerHandler, 
            CloudWatchMetricRecorder metricRecorder,
            ILogger<CloudWatchMetricEventHandlerDecorator<TEvent>> logger,
            bool isInMemoryHandler
            )
        {
            _innerHandler = innerHandler;
            _metricRecorder = metricRecorder;
            _logger = logger;
            _isInMemoryHandler = isInMemoryHandler;
        }

        public async Task Handle(TEvent @event)
        {
            await HandleDecorated(@event);
        }

        public async Task HandleDecorated(TEvent @event)
        {
            // Record metrics for handling events
            var metricName = $"HandledEvents_InMemory_{@event.GetType().Name}";

            if(_innerHandler == null) 
                throw new Exception($"No Inner handler is found for event {@event.GetType().Name}");


            var dataToLog = new
            {
                EventName = @event.GetType().Name,
                HandlerName = _innerHandler.GetType().Name,
                Payload = @event
            };

            _logger.LogInformationWithObject("Handling event in event handler", dataToLog);

            try
            {
                await _innerHandler.Handle(@event);
            }
            catch (DomainException ex)
            {
                _logger.LogWarningWithObject(ex.Message, dataToLog);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithObject(ex, ex.Message, dataToLog);
                throw;
            }

            _logger.LogInformationWithObject("Event handling Successful", new
            {
                EventName = @event.GetType().Name,
                HandlerName = _innerHandler.GetType().Name
            });

            if (!_isInMemoryHandler)
            {
                metricName = $"HandledEvents_SNS_{@event.GetType().Name}";
            }

            await _metricRecorder.RecordCloudWatchMetric(metricName, 1, @event.GetType().Name);
        }
    }

}
