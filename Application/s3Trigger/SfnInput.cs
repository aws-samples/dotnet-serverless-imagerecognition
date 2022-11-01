using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace s3Trigger
{
    public class SfnInput
    {
        public string Bucket { get; set; }
        public string SourceKey { get; set; }
        public string PhotoId { get; set; }
        public string UserId { get; set; }
        public string TablePhoto { get; set; }
    }
}
