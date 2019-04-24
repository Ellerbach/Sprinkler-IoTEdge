using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DarkSkyApi.Models;

namespace SprinklerNetCore.Models
{
    public class NeedToSrpinkle
    {
        [Display(Name = nameof(Resources.Text.NeedTo), ResourceType = typeof(Resources.Text))]
        public bool NeedTo { get; set; }
        [Display(Name = nameof(Resources.Text.PercentageCorrection), ResourceType = typeof(Resources.Text))]
        public float PercentageCorrection { get; set; }
        // Need to add the forecast data here
        [Display(Name = nameof(Resources.Text.Forecast), ResourceType = typeof(Resources.Text))]
        public DarkSkyApi.Models.Forecast Forecast { get; set; }
    }
}
