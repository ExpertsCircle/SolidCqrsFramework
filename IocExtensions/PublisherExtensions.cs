using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using SolidCqrsFramework.EventManagement;

namespace SolidCqrsFramework.IocExtensions
{
    public static class PublisherExtensions
    {
        /// <summary>
        /// Registers JustSaying and configures publishers for every type in the supplied assemblies that implements INotification.
        /// </summary>
        public static IServiceCollection AddPublisher(this IServiceCollection services,
            RegionEndpoint region, params Assembly[] assembliesWithNotifications)
        {
            services.AddJustSaying(config =>
            {
                // Configure messaging to use the specified AWS region.
                config.Messaging(m =>
                {
                    // Note: the WithRegion extension expects a string, e.g. "eu-west-1"
                    m.WithRegion(region.SystemName);
                });

                // Register a topic for every notification type.
                config.Publications(publications =>
                {
                    var notificationTypes = GetTypesImplementingINotification(assembliesWithNotifications);
                    foreach (var notificationType in notificationTypes)
                    {
                        // Find the generic WithTopic<T>() method on the Publications configuration.
                        var withTopicMethod = publications.GetType()
                            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(m => m.Name == "WithTopic" &&
                                                 m.IsGenericMethod &&
                                                 m.GetParameters().Length == 0);
                        if (withTopicMethod != null)
                        {
                            var genericMethod = withTopicMethod.MakeGenericMethod(notificationType);
                            genericMethod.Invoke(publications, null);
                        }
                        else
                        {
                            throw new InvalidOperationException("Could not find a suitable WithTopic<T>() method on the Publications configuration.");
                        }
                    }
                });
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
                    if (notificationInterface.IsAssignableFrom(type) &&
                        !type.IsInterface &&
                        !type.IsAbstract)
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
