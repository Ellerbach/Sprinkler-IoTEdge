using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SprinklerNetCore.Models
{
    public class SprinklerProgram
    {
        public SprinklerProgram() {
            id = Guid.NewGuid();
        }

        public SprinklerProgram(DateTimeOffset dateTimeStart, TimeSpan duration, int sprinklerNumber) : this()
        {
            DateTimeStart = dateTimeStart;
            Duration = duration;
            Number = sprinklerNumber;
        }

        [Display(Name = nameof(Resources.Text.DateTimeStart), ResourceType = typeof(Resources.Text))]
        public DateTimeOffset DateTimeStart { get; set; }

        [Display(Name = nameof(Resources.Text.Duration), ResourceType = typeof(Resources.Text))]
        public TimeSpan Duration { get; set; }

        [Display(Name = nameof(Resources.Text.SprinklerNumber), ResourceType = typeof(Resources.Text))]
        public int Number { get; set; }
       
        [JsonIgnore]
        public Guid id { get; set; }
    }
}