using System;
using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace SolidCqrsFramework.DynamoDbData
{
    public class DateTimeTypeConverter : IPropertyConverter
    {
        public DynamoDBEntry ToEntry(object value)
        {
            DateTime.TryParse(value.ToString() ?? string.Empty, null, DateTimeStyles.RoundtripKind, out DateTime result);
            DateTime valueToStore = default;
            if(result != default)
                valueToStore = ((DateTime)value);
            
            //TODO:: we need to update each projection and set UpdatedOn to be the event date

            DynamoDBEntry entry = new Primitive
                { Value = valueToStore.ToUniversalTime().ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ") };

            return entry;
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            if (entry is Primitive primitive)
            {
                var dtString = primitive.Value.ToString();
                var value = DateTime.Parse(dtString ?? string.Empty, null, DateTimeStyles.RoundtripKind);
                return value;
            }

            return null;
        }
    }

    public class DateTimeConverter : IPropertyConverter
    {
        public DynamoDBEntry ToEntry(object value)
        {
            var dateTime = (DateTime)value;

            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            var unixTime = entry.AsLong();

            var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTime);

            return dateTimeOffset.UtcDateTime;
        }
    }
}
