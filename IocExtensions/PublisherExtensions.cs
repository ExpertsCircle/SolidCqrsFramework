using Amazon;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.Aws;
using SolidCqrsFramework.EventManagement.Publishing;

namespace SolidCqrsFramework.IocExtensions
{
    public static class PublisherExtensions
    {
        /// <summary>
        /// Registers sns publisher 
        /// </summary>
        public static IServiceCollection AddPublisher(this IServiceCollection services, RegionEndpoint region)
        {
            // Automatically retrieve the accountId from our utility class.
            string accountId = AwsUtils.AccountId;

            services.AddSingleton<ISnsMessagePublisher>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<SnsMessagePublisher>>();
                var snsClient = new AmazonSimpleNotificationServiceClient(region);
                return new SnsMessagePublisher(snsClient, logger, region.SystemName, accountId);
            });
     
            return services;
        }
    }
}
