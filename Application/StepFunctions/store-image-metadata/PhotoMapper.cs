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
        public static Dictionary<string, AttributeValue> ToDynamoDBAttributes(this Photo photo)
        {
            int status = (int)photo.ProcessingStatus;
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>();

            item.Add("PhotoId", new AttributeValue(WebUtility.UrlDecode(photo.PhotoId)));
            item.Add("ProcessingStatus", new AttributeValue { N = status.ToString() });
            item.Add("FullSize", new AttributeValue { M = photo.FullSize.ToDynamoAttributes() });
            item.Add("Format", new AttributeValue { S = photo.Format });
            item.Add("ExifMake", new AttributeValue { S = photo.ExifMake });
            item.Add("ExifModel", new AttributeValue { S = photo.ExifModel });
            item.Add("Thumbnail", new AttributeValue { M = photo.Thumbnail.ToDynamoAttributes() });
            item.Add("ObjectDetected", new AttributeValue { SS =  photo.ObjectDetected.ToList()});
            item.Add("UpdatedDate", new AttributeValue { S = photo.UpdatedDate.ToString() });

            return item;
        }

        private static Dictionary<string, AttributeValue> ToDynamoAttributes(this PhotoImage photoImage)
        {
            Dictionary<String, AttributeValue> item = new Dictionary<string, AttributeValue>();
            item.Add("Key", new AttributeValue { S = photoImage.Key });
            item.Add("Width", new AttributeValue { N = photoImage.Width.ToString() });
            item.Add("Height", new AttributeValue { N = photoImage.Height.ToString() });

            return item;
        }
    }
}
