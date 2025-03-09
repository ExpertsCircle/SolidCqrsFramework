namespace SolidCqrsFramework.EventManagement.Publishing
{
    /// <summary>
    /// Custom message attribute class that aligns with SNS attribute properties.
    /// </summary>
    public class MessageAttributeValue
    {
        public string DataType { get; set; }
        public string StringValue { get; set; }
        // Note: Using byte[] ensures we can easily construct a MemoryStream.
        public byte[] BinaryValue { get; set; }
    }
}

