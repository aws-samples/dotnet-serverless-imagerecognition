using System;

namespace ImageRecognition.Frontend.Models
{
    public class GeoLocation
    {
        public Coordinate Latitude { get; set; }

        public Coordinate Longtitude { get; set; }

        public override string ToString()
        {
            return $"{Latitude.D}°{Math.Round(Latitude.M)}'{Math.Round(Latitude.S)}''{Latitude.Direction}" +
                   $" {Longtitude.D}°{Math.Round(Longtitude.M)}'{Math.Round(Longtitude.S)}''{Longtitude.Direction}";
        }
    }
}