using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.StepFunctions.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3Trigger
{
    internal static class StepFunctionResponseExtension
    {
        public static Dictionary<String, AttributeValueUpdate> ToDynamoDBAttributes(this StartExecutionResponse stepResponse)
        {
            int status = (int)ProcessingStatus.Running;

            Dictionary<String, AttributeValueUpdate> items = new Dictionary<string, AttributeValueUpdate>();

            items["SfnExecutionArn"] = ConvertToAttributeUpdateValue(stepResponse.ExecutionArn);
            items["ProcessingStatus"] = ConvertToAttributeUpdateValue(status.ToString());
            items["UpdatedDate"] = ConvertToAttributeUpdateValue(DateTime.UtcNow.ToString());

            return items;
        }

        internal static AttributeValueUpdate ConvertToAttributeUpdateValue(string attributeValue)
        {
            AttributeValueUpdate attributeUpdate = new AttributeValueUpdate();

            Console.WriteLine(attributeValue);

            if (attributeValue == null)
            {
                attributeUpdate.Action = "DELETE";
            }
            else
            {
                attributeUpdate.Action = "PUT";
                attributeUpdate.Value = new AttributeValue { S = attributeValue };
            }

            return attributeUpdate;
        }
    }
}
