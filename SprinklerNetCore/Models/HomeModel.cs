using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprinklerNetCore.Models
{
    public class HomeModel
    {
        public List<Sprinkler> Sprinklers { get; set; }
        public NeedToSrpinkle NeedToSrpinkle { get; set; }
        public string DateTime { get; set; }
    }
}
