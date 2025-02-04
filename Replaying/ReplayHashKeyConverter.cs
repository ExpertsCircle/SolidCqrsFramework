using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace SolidCqrsFramework.Replaying
{
    public class ReplayHashKeyConverter : IPropertyConverter
    {
        private readonly string _replayPrefix = "REPLAYER#";

        public DynamoDBEntry ToEntry(object value)
        {
            // Prefix the hash key value with "REPLAYER#"
            if (value == null) throw new ArgumentNullException(nameof(value));

            DynamoDBEntry entry = new Primitive
            {
                Value = $"{_replayPrefix}{value.ToString()?.ToLowerInvariant()}"
            };

            return entry;
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            // Extract the original value by removing the "REPLAYER#" prefix
            var primitive = entry as Primitive;
            if (!(primitive?.Value is string) || string.IsNullOrEmpty((string)primitive.Value))
                throw new ArgumentOutOfRangeException();

            var data = ((string)primitive.Value).Split(new[] { "#" }, StringSplitOptions.None);
            if (data.Length != 2 || data[0] != "REPLAYER") throw new ArgumentOutOfRangeException();

            return data[1]; // Return the original value without the prefix
        }
    }
}


