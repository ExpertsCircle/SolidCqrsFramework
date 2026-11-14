using System;
using System.Collections.Generic;
using SolidCqrsFramework.EventManagement;

namespace SolidCqrsFramework
{
    /// <summary>
    /// Whether the framework's own log lines carry the body of a command or event.
    /// <para>
    /// Off by default. Commands and events hold personal data (names, email addresses, phone numbers,
    /// CV file names, invoice addresses, free-text notes), and every log line is kept in CloudWatch.
    /// With it off, the framework logs the message type name and, for an event, its id, aggregate id
    /// and conversation (correlation) id. Error and warning lines keep the exception and its message.
    /// </para>
    /// <para>
    /// Opt in for local debugging only: set the environment variable
    /// <c>SOLIDCQRS_LOG_MESSAGE_BODIES=true</c>, or set <see cref="Enabled"/> in code. The environment
    /// variable is ignored inside AWS Lambda (<c>AWS_LAMBDA_FUNCTION_NAME</c> set), so a stray
    /// setting on a deployed function cannot turn it on.
    /// </para>
    /// </summary>
    public static class MessageBodyLogging
    {
        public const string EnvironmentVariable = "SOLIDCQRS_LOG_MESSAGE_BODIES";

        public static bool Enabled { get; set; } = FromEnvironment(Environment.GetEnvironmentVariable);

        public static bool FromEnvironment(Func<string, string> getVariable)
        {
            if (!string.IsNullOrEmpty(getVariable("AWS_LAMBDA_FUNCTION_NAME")))
                return false;

            var value = getVariable(EnvironmentVariable);
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
        }

        internal static IDictionary<string, object> ForCommand(object command, string handlerName = null)
        {
            var fields = new Dictionary<string, object> { ["CommandName"] = command?.GetType().Name };
            if (handlerName != null)
                fields["HandlerName"] = handlerName;
            if (Enabled)
                fields["Message"] = command;
            return fields;
        }

        internal static IDictionary<string, object> ForEvent(Type eventType, object message, string handlerName = null)
        {
            var fields = new Dictionary<string, object> { ["EventName"] = (eventType ?? message?.GetType())?.Name };
            if (handlerName != null)
                fields["HandlerName"] = handlerName;
            if (message is Event @event)
            {
                fields["EventId"] = @event.Id;
                fields["AggregateId"] = @event.AggregateId;
                if (!string.IsNullOrEmpty(@event.Conversation))
                    fields["Conversation"] = @event.Conversation;
            }
            if (Enabled)
                fields["Payload"] = message;
            return fields;
        }
    }
}
