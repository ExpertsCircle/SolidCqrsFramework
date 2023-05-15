using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon;
using JustSaying;
using JustSaying.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.EventManagement;


namespace SolidCqrsFramework.IocExtensions
{
    public static class PublisherExtensions
    {
        public static IServiceCollection AddPublisher(this IServiceCollection services,
            RegionEndpoint region, params Assembly[] assembliesWithNotifications)
        {
            services.TryAddSingleton<IMessagePublisher>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var publisher = CreateMeABus
                    .WithLogging(loggerFactory)
                    .InRegion(region.SystemName);

                var types = GetTypesImplementingINotification(assembliesWithNotifications);

                foreach (var notificationType in types)
                {
                    var method = publisher.GetType()
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(m => m.Name == "WithSnsMessagePublisher" && m.GetParameters().Length == 0)
                        ?.MakeGenericMethod(notificationType);

                    if (method != null)
                    {
                        method.Invoke(publisher, null);
                    }
                }

                return publisher;
            });

            return services;
        }

        private static IEnumerable<Type> GetTypesImplementingINotification(params Assembly[] assemblies)
        {
            var notificationInterface = typeof(INotification);

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (notificationInterface.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        yield return type;
                    }
                }
            }
        }
    }

}
