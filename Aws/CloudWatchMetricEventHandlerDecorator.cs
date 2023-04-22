using System.Threading.Tasks;
using SolidCqrsFramework.EventManagement;

namespace SolidCqrsFramework.Aws
{
    public class CloudWatchMetricEventHandlerDecorator<TEvent> : IEventHandlerDecorator<TEvent> where TEvent : Event
    {
        private readonly IEventHandler<TEvent> _innerHandler;
        private readonly CloudWatchMetricRecorder _metricRecorder;

        public CloudWatchMetricEventHandlerDecorator(IEventHandler<TEvent> innerHandler, CloudWatchMetricRecorder metricRecorder)
        {
            _innerHandler = innerHandler;
            _metricRecorder = metricRecorder;
        }

        public async Task Handle(TEvent @event)
        {
            await HandleDecorated(@event);
        }

        public async Task HandleDecorated(TEvent @event)
        {
            // Record metrics for handling events
            await _metricRecorder.RecordCloudWatchMetric("PublishedEvents", 1, @event.GetType().Name);

            // Call the actual handler
            await _innerHandler.Handle(@event);
        }
    }
}
