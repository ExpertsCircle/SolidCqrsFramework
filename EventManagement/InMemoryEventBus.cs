using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SolidCqrsFramework.EventManagement
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _dict;

        public InMemoryEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dict = new Dictionary<Type, object>();
        }

        private async Task Publish<T>(T @event) where T : Event
        {
            var evenType = @event.GetType();
           // var handlerType = typeof(IEventHandler<>).MakeGenericType(evenType);

            var handlers = GetHandlers(evenType);
           // dynamic handlers = _container.GetServices(handlerType);
            foreach (var eventHandler in handlers)
            {
                try
                {
                    await (Task) eventHandler.Handle((dynamic)@event);
                }
                catch (Exception e)
                {
                    //Policy.Handle<Exception>()
                    //    .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200) })
                    //    .ExecuteAsync(await eventHandler.Handle((dynamic)@event));
                    Console.WriteLine(e);

                    throw;
                }
            }
        }

        private dynamic GetHandlers(Type eventType)
        {
            if (_dict.ContainsKey(eventType))
                return _dict[eventType];
            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerType);
            _dict.Add(eventType, handlers);
            return handlers;
        }

        public async Task Publish<T>(IEnumerable<T> events) where T : Event
        {
            foreach (var @event in events)
            {
                var desEvent = (T) Convert.ChangeType(@event, @event.GetType());

                if(desEvent is IInProcessEvent)
                    await Publish(desEvent);
                else
                    await Publish(desEvent);//PublishToKinesis(desEvent);

            }
        }

        private Task PublishToKinesis<T>(T desEvent) where T : Event
        {
            throw new NotImplementedException();
        }
    }
}
