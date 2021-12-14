using System;
using Amazon.DynamoDBv2.DataModel;

namespace s3Trigger
{
    public enum ProcessingStatus
    {
        Pending = 0,
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Timed_Out = 4,
        Aborted = 5
    }

    public class Photo
    {
        [DynamoDBHashKey] public string PhotoId { get; set; }

        [DynamoDBProperty] public ProcessingStatus ProcessingStatus { get; set; }

        [DynamoDBProperty] public string SfnExecutionArn { get; set; }

        [DynamoDBProperty] public DateTime? UpdatedDate { get; set; }
    }
}