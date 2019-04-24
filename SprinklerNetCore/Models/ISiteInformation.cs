using System.Collections.Generic;

namespace SprinklerNetCore.Models
{
    public interface ISiteInformation
    {
        Settings Settings { get; set; }
        List<FuzzySprinkler> FuzzySprinklers { get; set; }
        int MaxSprinklers { get; }
        NeedToSrpinkle NeedToSprinkle { get; set; }
        SoilHumidity SoilHumidity { get; set; }
        List<SprinklerProgram> SprinklerPrograms { get; set; }
        List<Sprinkler> Sprinklers { get; set; }
    }
}