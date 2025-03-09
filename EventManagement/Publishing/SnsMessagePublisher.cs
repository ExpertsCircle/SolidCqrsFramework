using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using JustSaying.Models;
using Microsoft.Extensions.Logging;

namespace SolidCqrsFramework.EventManagement.Publishing
{
    /// <summary>
    /// Minimal SNS publisher that:
    /// 1) Serializes messages using System.Text.Json.
    /// 2) Builds the SNS topic ARN using a convention:
    ///    arn:aws:sns:{region}:{accountId}:{messageTypeName}.
    /// 3) Converts custom message attributes into the AWS SNS model.
    /// </summary>
    public class SnsMessagePublisher : ISnsMessagePublisher
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly ILogger<SnsMessagePublisher> _logger;
        private readonly string _region;
        private readonly string _accountId;

        // Configure JSON to mimic JustSaying's typical style (camelCase, ignore nulls).
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public SnsMessagePublisher(
            IAmazonSimpleNotificationService snsClient,
            ILogger<SnsMessagePublisher> logger,
            string region,
            string accountId)
        {
            _snsClient = snsClient;
            _logger = logger;
            _region = region;
            _accountId = accountId;
        }

        public Task PublishAsync(Message message, CancellationToken cancellationToken)
            => PublishAsync(message, null, cancellationToken);

        public async Task PublishAsync(Message message, PublishMetadata metadata, CancellationToken cancellationToken)
        {
            var request = BuildPublishRequest(message, metadata);
            try
            {
                var response = await _snsClient.PublishAsync(request, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation(
                    "Published message {MessageId} of type {MessageType} to SNS topic {TopicArn}.",
                    message.Id,
                    message.GetType().FullName,
                    request.TopicArn);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing message {MessageId} to SNS topic {TopicArn}.",
                    message.Id,
                    request.TopicArn);
                throw new Exception($"Failed to publish message to SNS topic '{request.TopicArn}'.", ex);
            }
        }

        /// <summary>
        /// Builds the PublishRequest by serializing the message and mapping attributes.
        /// </summary>
        private PublishRequest BuildPublishRequest(Message message, PublishMetadata metadata)
        {
            // 1) Serialize the message using System.Text.Json.            
            string payload = JsonSerializer.Serialize(message, message.GetType(), SerializerOptions);
            // 2) Use the message type name as the subject and part of the topic ARN.
            string topicName = message.GetType().Name;
            string subject = topicName;
            string topicArn = $"arn:aws:sns:{_region}:{_accountId}:{topicName.ToLower()}";

            // 3) Construct the final request.
            return new PublishRequest
            {
                TopicArn = topicArn,
                Message = payload,
                Subject = subject,
                MessageAttributes = BuildMessageAttributes(metadata)
            };
        }

        /// <summary>
        /// Converts our custom message attributes into the AWS SDK's MessageAttributeValue dictionary.
        /// </summary>
        private static Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue> BuildMessageAttributes(PublishMetadata metadata)
        {
            if (metadata?.MessageAttributes == null || metadata.MessageAttributes.Count == 0)
                return null;

            return metadata.MessageAttributes.ToDictionary(
                kvp => kvp.Key,
                kvp => new Amazon.SimpleNotificationService.Model.MessageAttributeValue
                {
                    DataType = kvp.Value.DataType,
                    StringValue = kvp.Value.StringValue,
                    BinaryValue = kvp.Value.BinaryValue == null
                        ? null
                        : new MemoryStream(kvp.Value.BinaryValue, writable: false)
                }
            );
        }
    }
}
