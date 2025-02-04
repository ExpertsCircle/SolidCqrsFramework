using System;
using Amazon.DynamoDBv2.DataModel;

namespace SolidCqrsFramework.DynamoDbData
{
    public abstract class BaseData
    {
        [DynamoDBProperty(typeof(DateTimeTypeConverter))]
        public DateTime UpdatedOn { get; set; }

        [Obsolete("Warning: Setting this will delete this record. Only set it if you want to delete it ear;er then the default value that comes from environment variable.")]
        [DynamoDBProperty(Converter = typeof(TtlConverter))]
        public DateTime? Expires { get; set; } = GetTTL();

        private static DateTime? GetTTL()
        {
            var hasTtlMinutesDefined = int.TryParse(Environment.GetEnvironmentVariable("dynamodb_ttl_minutes"), out var ttlMinutes);
            return hasTtlMinutesDefined ? DateTime.UtcNow.AddMinutes(ttlMinutes)
                : DateTime.UtcNow.AddMonths(DynamoDbMetaData.TtlMonthsForGeneralData);
        }
    }
}
