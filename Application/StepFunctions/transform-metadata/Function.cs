using System;
using Amazon.Lambda.Core;
using Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace transform_metadata
{
    public class Function
    {
        /// <summary>
        ///     A simple function that takes a string and returns both the upper and lower case version of the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public TransformedMetadata FunctionHandler(ImageMetadata extractedMetadata, ILambdaContext context)
        {
            ExifProfile exifProfile = null;
            if (!string.IsNullOrEmpty(extractedMetadata.ExifProfileBase64))
                exifProfile = new ExifProfile(Convert.FromBase64String(extractedMetadata.ExifProfileBase64));

            var transformedMetadata = new TransformedMetadata
            {
                CreationTime = DateTime.Now,
                Format = extractedMetadata.Format,
                Dimensions = new Dimensions
                {
                    Height = extractedMetadata.Height,
                    Width = extractedMetadata.Width
                },

                Geo = ExtractGeoLocation(exifProfile),
                ExifMake = exifProfile?.GetValue(ExifTag.Make)?.Value,
                ExifModel = exifProfile?.GetValue(ExifTag.Model)?.Value,
                FileSize = extractedMetadata.Size
            };

            return transformedMetadata;
        }

        private GeoLocation ExtractGeoLocation(ExifProfile exifProfile)
        {
            if (exifProfile?.GetValue(ExifTag.GPSLatitude) == null)
                // no GPS exifProfile found.
                return null;

            var geo = new GeoLocation
            {
                Latitude = ParseCoordinate(exifProfile.GetValue(ExifTag.GPSLatitudeRef)?.Value,
                    exifProfile.GetValue(ExifTag.GPSLatitude)?.Value),

                Longtitude = ParseCoordinate(exifProfile.GetValue(ExifTag.GPSLongitudeRef)?.Value,
                    exifProfile.GetValue(ExifTag.GPSLongitude)?.Value)
            };

            return geo;
        }

        private Coordinate ParseCoordinate(string gpsRef, Rational[] rationals)
        {
            var coordinate = new Coordinate
            {
                D = rationals[0].Numerator / rationals[0].Denominator,
                M = rationals[1].Numerator / rationals[1].Denominator,
                S = rationals[2].Numerator / rationals[2].Denominator,
                Direction = gpsRef
            };

            return coordinate;
        }
    }
}