using System.Collections.Generic;

namespace SolidCqrsFramework.EventManagement.Publishing
{
    /// <summary>
    /// A simple container for optional publish metadata, including message attributes.
    /// </summary>
    public class PublishMetadata
    {
        public IDictionary<string, MessageAttributeValue> MessageAttributes { get; set; }
    }
}

