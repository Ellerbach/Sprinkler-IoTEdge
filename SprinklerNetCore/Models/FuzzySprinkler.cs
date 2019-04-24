using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace SprinklerNetCore.Models
{
    public class FuzzySprinkler
    {
        public FuzzySprinkler()
        {
            id = Guid.NewGuid();
        }

        [Display(Name = nameof(Resources.Text.TempMin), ResourceType = typeof(Resources.Text))]
        public float TempMin { get; set; }
        [Display(Name = nameof(Resources.Text.TempMax), ResourceType = typeof(Resources.Text))]
        public float TempMax { get; set; }
        [Display(Name = nameof(Resources.Text.RainMax), ResourceType = typeof(Resources.Text))]
        public float RainMax { get; set; }
        [Display(Name = nameof(Resources.Text.SprinklingMax), ResourceType = typeof(Resources.Text))]
        public float SprinklingMax { get; set; }

        [JsonIgnore]
        public Guid id { get; set; }
    }
}
