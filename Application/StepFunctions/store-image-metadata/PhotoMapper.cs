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
            item.Add("FullSize", new AttributeValue { M = photo.FullSize.ToDynamoDocument().ToAttributeMap() });
            item.Add("Format", new AttributeValue { S = photo.Format });
            item.Add("ExifMake", new AttributeValue { S = photo.ExifMake });
            item.Add("ExifModel", new AttributeValue { S = photo.ExifModel });
            item.Add("Thumbnail", new AttributeValue { M = photo.Thumbnail.ToDynamoDocument().ToAttributeMap() });
            item.Add("ObjectDetected", new AttributeValue { SS =  photo.ObjectDetected.ToList()});
            item.Add("UpdatedDate", new AttributeValue { S = photo.UpdatedDate.ToString() });

            return item;
        }

        private static Document ToDynamoDocument(this PhotoImage photoImage)
        {
            var document = new Document();
            document["Key"] = photoImage.Key;
            document["Width"] = photoImage.Width;
            document["Height"] = photoImage.Height;

            return document;
        }
    }
}
