using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JustSaying.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.EventManagement;

namespace SolidCqrsFramework.Aws
{
    public class InMemoryWithAwsEventBus : IEventBus
    {
        private readonly CloudWatchMetricRecorder _metricRecorder;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _dict;
        private readonly IMessagePublisher _awsMessagePublisher;
        private readonly ILogger<InMemoryEventBus> _logger;

        public InMemoryWithAwsEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger, 
            CloudWatchMetricRecorder metricRecorder, IMessagePublisher awsMessagePublisher)
        {
            _metricRecorder = metricRecorder;
            _awsMessagePublisher = awsMessagePublisher;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dict = new Dictionary<Type, object>();
            _serviceProvider.GetRequiredService<IMessagePublisher>();
        }

        private async Task Publish<T>(T @event) where T : Event
        {
            var evenType = @event.GetType();
            var handlers = GetDecoratedHandlers(evenType);

            foreach (var eventHandler in handlers)
            {
                try
                {
                    await (Task) eventHandler.Handle((dynamic)@event);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to execute event handler");
                    throw;
                }
            }
        }


        private dynamic GetDecoratedHandlers(Type eventType)
        {
            if (_dict.ContainsKey(eventType))
                return _dict[eventType];

            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerType);

            var decoratedHandlers = new List<object>();
            foreach (var handler in handlers)
            {
                var decoratorType = typeof(CloudWatchMetricEventHandlerDecorator<>).MakeGenericType(eventType);
                var decorator = Activator.CreateInstance(decoratorType, handler);
                decoratedHandlers.Add(decorator);
            }

            _dict.Add(eventType, decoratedHandlers);
            return decoratedHandlers;
        }


        public async Task Publish<T>(IEnumerable<T> events) where T : Event
        {
            foreach (var @event in events)
            {
                var desEvent = (T) Convert.ChangeType(@event, @event.GetType());

                if (desEvent is INotification)
                {
                    await PublishToAws(desEvent);
                    await _metricRecorder.RecordCloudWatchMetric($"PublishedEvents_to_SNS_{desEvent.GetType().Name}", 1, desEvent.GetType().Name);
                }
                else
                {
                    await Publish(desEvent);
                    await _metricRecorder.RecordCloudWatchMetric($"PublishedEvents_to_Memory_{desEvent.GetType().Name}", 1, desEvent.GetType().Name);
                }
            }
        }

        private async Task PublishToAws<T>(T desEvent) where T : Event
        {
            await _awsMessagePublisher.PublishAsync(desEvent);
        }
    }
}
