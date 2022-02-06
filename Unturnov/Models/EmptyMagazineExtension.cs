using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class EmptyMagazineExtension : ItemExtension
    {
        public List<LoadedMagazineVariant> LoadedMagazines;

        public class LoadedMagazineVariant : ItemExtension
        {
            public byte RefillAmmoBlueprintIndex;
        }
    }

}
