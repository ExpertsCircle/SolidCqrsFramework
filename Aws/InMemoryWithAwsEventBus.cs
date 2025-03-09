using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.EventManagement;
using SolidCqrsFramework.EventManagement.Publishing;

namespace SolidCqrsFramework.Aws
{
    public class InMemoryWithAwsEventBus : IEventBus
    {
        private readonly CloudWatchMetricRecorder _metricRecorder;
        private readonly ISnsMessagePublisher _awsMessagePublisher;
        private readonly EventProcessor _eventProcessor;
        private readonly ILogger<InMemoryEventBus> _logger;

        public InMemoryWithAwsEventBus(ILogger<InMemoryEventBus> logger,
            CloudWatchMetricRecorder metricRecorder,
            ISnsMessagePublisher awsMessagePublisher,
            IServiceProvider serviceProvider)
        {
            _metricRecorder = metricRecorder;
            _awsMessagePublisher = awsMessagePublisher;
            _eventProcessor = new EventProcessor(serviceProvider, true);
            _logger = logger;
        }


        public async Task Publish<T>(IEnumerable<T> events) where T : Event
        {
            foreach (var @event in events)
            {
                _logger.LogInformationWithObject("Publishing event", new {EventName = @event.GetType().Name });
                var desEvent = (T)Convert.ChangeType(@event, @event.GetType());

                //await Publish(desEvent);
                await _metricRecorder.RecordCloudWatchMetric($"PublishedEvents_to_Memory_{desEvent.GetType().Name}", 1, desEvent.GetType().Name);

                await _eventProcessor.ProcessEventAsync(desEvent.GetType(), desEvent);

                if (desEvent is INotification)
                {
                    await PublishToAws(desEvent);
                    _logger.LogInformationWithObject("Published event to AWS", new { EventName = @event.GetType().Name });
                    await _metricRecorder.RecordCloudWatchMetric($"PublishedEvents_to_SNS_{desEvent.GetType().Name}", 1, desEvent.GetType().Name);
                }

            }
        }

        private async Task PublishToAws<T>(T desEvent) where T : Event
        {
            await _awsMessagePublisher.PublishAsync(desEvent, CancellationToken.None);
        }
    }
}
