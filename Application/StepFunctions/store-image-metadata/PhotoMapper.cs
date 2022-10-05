using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace store_image_metadata
{
    public static class ProtoMapper {
        public static Dictionary<string, AttributeValueUpdate> ToDynamoDBAttributes(this Photo photo)
        {
            int status = (int)photo.ProcessingStatus;
            Dictionary<String, AttributeValueUpdate> item = new Dictionary<string, AttributeValueUpdate>();

            //item["PhotoId"] = ConvertToStringAttributeUpdateValue(photo.PhotoId);
            item["ProcessingStatus"] = ConvertToNumberAttributeUpdateValue(status.ToString());
            item["Format"] = ConvertToStringAttributeUpdateValue(photo.PhotoId);
            item["UpdatedDate"] = ConvertToStringAttributeUpdateValue(photo.PhotoId);
            item["FullSize"] = ConvertToMapAttributeUpdateValue (photo.FullSize.ToDynamoAttributes());
            item["Thumbnail"] = ConvertToMapAttributeUpdateValue(photo.Thumbnail.ToDynamoAttributes());

            if (photo.ExifMake != null)
            {
                item["ExifMake"] = ConvertToStringAttributeUpdateValue(photo.ExifMake);
            }
            if (photo.ExifModel != null)
            {
                item["ExifModel"] = ConvertToStringAttributeUpdateValue(photo.ExifModel);
            }
            if (photo.ObjectDetected != null)
            {
                item["ObjectDetected"] = ConvertToArrayAttributeUpdateValue(photo.ObjectDetected.ToList());
            }
            return item;
        }

        private static Dictionary<string, AttributeValue> ToDynamoAttributes(this PhotoImage photoImage)
        {
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>();
            item["Key"] = ConvertToAttributeValue(photoImage.Key);
            if (photoImage.Width != null)
            {
                item["Width"] = ConvertToAttributeValue(photoImage.Width.ToString());
            }
            if (photoImage.Height != null)
            {
                item["Height"] = ConvertToAttributeValue(photoImage.Height.ToString());
            }
            return item;
        }

        internal static AttributeValue ConvertToAttributeValue(string attributeValue)
        {
            return new AttributeValue
            {
                S = attributeValue
            };
        }

        internal static AttributeValueUpdate ConvertToStringAttributeUpdateValue(string attributeValue)
        {
            return new AttributeValueUpdate
            {
                Action = "PUT",
                Value = new AttributeValue { S = attributeValue }
            };
        }

        internal static AttributeValueUpdate ConvertToNumberAttributeUpdateValue(string attributeValue)
        {
            return new AttributeValueUpdate
            {
                Action = "PUT",
                Value = new AttributeValue { N = attributeValue }
            };
        }

        internal static AttributeValueUpdate ConvertToArrayAttributeUpdateValue(List<string> attributeValue)
        {
            return new AttributeValueUpdate
            {
                Action = "PUT",
                Value = new AttributeValue { SS = attributeValue }
            };
        }

        internal static AttributeValueUpdate ConvertToMapAttributeUpdateValue(Dictionary<string, AttributeValue> attributeValue)
        {
            return new AttributeValueUpdate
            {
                Action = "PUT",
                Value = new AttributeValue { M = attributeValue }
            };
        }
    }
}
