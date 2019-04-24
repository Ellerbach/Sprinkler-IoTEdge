using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SprinklerNetCore.Models
{
    public class TypicalProgram
    {
        [Display(Name = nameof(Resources.Text.Duration), ResourceType = typeof(Resources.Text))]
        public TimeSpan Duration { get; set; }
        [Display(Name = nameof(Resources.Text.StartTime), ResourceType = typeof(Resources.Text))]
        public TimeSpan StartTime { get; set; }
    }
}
