// using DarkSkyApi;

using DarkSkyApi;
using System;
using System.ComponentModel.DataAnnotations;

namespace SprinklerNetCore.Models
{
    public class Settings
    {
        [Display(Name = nameof(Resources.Text.TimeZoneInfo), ResourceType = typeof(Resources.Text))]
        public String TimeZoneInfoId { get; set; }
        [Display(Name = nameof(Resources.Text.ApiKey), ResourceType = typeof(Resources.Text))]
        public string ApiKey { get; set; }
        [Display(Name = nameof(Resources.Text.Latitude), ResourceType = typeof(Resources.Text))]
        public float Latitude { get; set; }
        [Display(Name = nameof(Resources.Text.Longitude), ResourceType = typeof(Resources.Text))]
        public float Longitude { get; set; }
        [Display(Name = nameof(Resources.Text.Unit), ResourceType = typeof(Resources.Text))]
        public Unit Unit { get; set; }
        [Display(Name = nameof(Resources.Text.Language), ResourceType = typeof(Resources.Text))]
        public Language Language { get; set; }
        [Display(Name = nameof(Resources.Text.City), ResourceType = typeof(Resources.Text))]
        public string City { get; set; }
        [Display(Name = nameof(Resources.Text.TimeToCheck), ResourceType = typeof(Resources.Text))]
        public TimeSpan TimeToCheck { get; set; }
        [Display(Name = nameof(Resources.Text.FullAutomationMode), ResourceType = typeof(Resources.Text))]
        public bool FullAutomationMode { get; set; }
        [Display(Name = nameof(Resources.Text.PrecipitationThresholdActuals), ResourceType = typeof(Resources.Text))]
        public float PrecipitationThresholdActuals { get; set; }
        [Display(Name = nameof(Resources.Text.PrecipitationThresholdForecast), ResourceType = typeof(Resources.Text))]
        public float PrecipitationThresholdForecast { get; set; }
        [Display(Name = nameof(Resources.Text.PrecipitationPercentForecast), ResourceType = typeof(Resources.Text))]
        public int PrecipitationPercentForecast { get; set; }
        [Display(Name = nameof(Resources.Text.Name), ResourceType = typeof(Resources.Text))]
        public string Name { get; set; }
    }
}
