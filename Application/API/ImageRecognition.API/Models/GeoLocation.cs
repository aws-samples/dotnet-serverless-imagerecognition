using System;

namespace ImageRecognition.API.Models
{
    public class GeoLocation
    {
        public Coordinate Latitude { get; set; }

        public Coordinate Longitude { get; set; }

        public override string ToString()
        {
            return $"{Latitude.D}°{Math.Round(Latitude.M)}'{Math.Round(Latitude.S)}''{Latitude.Direction}" +
                   $" {Longitude.D}°{Math.Round(Longitude.M)}'{Math.Round(Longitude.S)}''{Longitude.Direction}";
        }
    }
}