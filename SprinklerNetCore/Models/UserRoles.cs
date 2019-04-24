using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SprinklerNetCore.Models
{
    public class UserRoles
    {
        [Display(Name = nameof(Resources.Text.Email), ResourceType = typeof(Resources.Text))]
        public string Email { get; set; }
        [Display(Name = nameof(Resources.Text.Roles), ResourceType = typeof(Resources.Text))]
        public List<string> Roles { get; set; }
    }
}
