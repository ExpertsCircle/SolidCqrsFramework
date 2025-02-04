using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace SolidCqrsFramework.DynamoDbData;

public class TtlConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object value)
    {
        var dateTime = (DateTime)value;
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        var unixTime = entry.AsLong();
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        return dateTimeOffset.UtcDateTime;
    }
}
