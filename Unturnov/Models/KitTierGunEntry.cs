using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    internal class KitTierGunEntry : KitTierEntry
    {
        public int SightWeightMin = 1;
        public int SightWeightMax = 10;
        public int TacticalWeightMin = 1;
        public int TacticalWeightMax = 10;
        public int GripsWeightMin = 1;
        public int GripsWeightMax = 10;
        public int MuzzleWeightMin = 1;
        public int MuzzleWeightMax = 10;
        public int MagsWeightMin = 1;
        public int MagsWeightMax = 10;
        public int SpareAmmoWeightMin = 1;
        public int SpareAmmoWeightMax = 10;

        public float MagCountChange = 1;
        public float AmmoCountChange = 1;
    }
}
