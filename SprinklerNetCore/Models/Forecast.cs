using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprinklerNetCore.Models
{
    public class Forecast
    {
        public string Summary { get; set; }
        public string Icon { get; set; }
        public List<Daily> Daily { get; set; }
        public Hourly Hourly { get; set; }
    }

    public class Daily
    {
        public DateTimeOffset Time { get; set; }
        public string Summary { get; set; }
        public string Icon { get; set; }
        public float LowTemperature { get; set; }
        public float HighTemperature { get; set; }
        public float WindSpeed { get; set; }
        public string PrecipitationType { get; set; }
        public float PrecipitationProbability { get; set; }
        public float PrecipitationIntensity { get; set; }
    }

    public class Hourly
    {
        public List<Hours> Hours { get; set; }
    }

    public class Hours
    {
        public DateTimeOffset Time { get; set; }
        public string Summary { get; set; }
        public string Icon { get; set; }
        public float Temperature { get; set; }
        public float ApparentTemperature { get; set; }
        public float WindSpeed { get; set; }
        public string PrecipitationType { get; set; }
        public float PrecipitationProbability { get; set; }
        public float PrecipitationIntensity { get; set; }
    }

}
