using DarkSkyApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprinklerNetCore.Resources
{
    public class Culture
    {
        public static string FromLanguageToCulture(Language language)
        {
            switch (language)
            {
                case Language.Arabic:
                    break;
                case Language.Azerbaijani:
                    break;
                case Language.Belarusian:
                    break;
                case Language.Bulgarian:
                    break;
                case Language.Bosnian:
                    break;
                case Language.Catalan:
                    break;
                case Language.Czech:
                    break;
                case Language.German:
                    break;
                case Language.Greek:
                    break;
                case Language.English:
                    break;
                case Language.Spanish:
                    break;
                case Language.Estonian:
                    break;
                case Language.French:
                    return "fr";
                case Language.Croatian:
                    break;
                case Language.Hungarian:
                    break;
                case Language.Indonesian:
                    break;
                case Language.Italian:
                    break;
                case Language.Icelandic:
                    break;
                case Language.Cornish:
                    break;
                case Language.NorwegianBokmal:
                    break;
                case Language.Dutch:
                    break;
                case Language.Polish:
                    break;
                case Language.Portuguese:
                    break;
                case Language.Russian:
                    break;
                case Language.Slovak:
                    break;
                case Language.Serbian:
                    break;
                case Language.Swedish:
                    break;
                case Language.Tetum:
                    break;
                case Language.Turkish:
                    break;
                case Language.Ukrainian:
                    break;
                case Language.PigLatin:
                    break;
                case Language.Chinese:
                    break;
                case Language.TraditionalChinese:
                    break;
                default:
                    break;
            }
            return "en-US";
        }
    }
}
