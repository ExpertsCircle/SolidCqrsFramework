using System;
using Amazon.DynamoDBv2.DataModel;
using SolidCqrsFramework.DynamoDbData;

namespace SolidCqrsFramework.Replaying;

public class ReplayRunData : BaseData
{
    [DynamoDBHashKey("Hash", typeof(ReplayHashKeyConverter))]
    public string ReplayId { get; set; } 

    [DynamoDBRangeKey("Range")]
    public string RecordType { get; set; }

    public string Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int EventCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FailedPayloadKeyAndType { get; set; }
    public DateTime? FailedEventTimestamp { get; set; }
}
