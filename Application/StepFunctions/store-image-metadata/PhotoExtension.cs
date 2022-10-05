using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace store_image_metadata
{
    public static class ProtoExtension {
        public static UpdateItemRequest ToDynamoDBUpdateRequest(this Photo photo, string photoTable)
        {
            int status = (int)photo.ProcessingStatus;

            var request = new UpdateItemRequest
            {
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "PhotoId", new AttributeValue { S = photo.PhotoId } }
                },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    { "#PS", "ProcessingStatus" },
                    { "#UD", "UpdatedDate" },
                    { "#FS", "FullSize" },
                    { "#TN", "Thumbnail" },
                    { "#OD", "ObjectDetected" },
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":status",new AttributeValue { S = status.ToString() }},
                    {":date",new AttributeValue { S = DateTime.UtcNow.ToString()}},
                    {":full",new AttributeValue { M = photo.FullSize.ToDynamoAttributes() }},
                    {":thumb",new AttributeValue { M = photo.Thumbnail.ToDynamoAttributes() }},
                    {":objects",new AttributeValue { SS = photo.ObjectDetected.ToList() }}
                },

                UpdateExpression = "SET #PS = :status, #UD =:date, #FS =:full, #TN =:thumb, #OD =:objects",

                TableName = photoTable,
                ReturnValues = "UPDATED_NEW"
            };

            return request;
        }

        private static Dictionary<string, AttributeValue> ToDynamoAttributes(this PhotoImage photoImage)
        {
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>();
            item.Add("Key", new AttributeValue { S = photoImage.Key });
            if (photoImage.Width != null)
            {
                item.Add("Width", new AttributeValue { N = photoImage.Width.ToString() });
            }
            if (photoImage.Height != null)
            {
                item.Add("Height", new AttributeValue { N = photoImage.Height.ToString() });
            }
            return item;
        }
    }
}
