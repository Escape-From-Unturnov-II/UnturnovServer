using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class ScavKitTier
    {
        public KitTierEntry HatConfig;
        public KitTierEntry GlassesConfig;
        public KitTierEntry VestConfig;
        public KitTierEntry BackpackConfig;
        public KitTierEntry ShirtConfig;
        public KitTierEntry PantsConfig;

        public KitTierEntry GunConfig;
        public KitTierEntry MedConfig;
        public KitTierEntry SupplyConfig;

    }

    public class KitTierEntry
    {
        public byte CountMin = 1;
        public byte CountMax = 1;

        public int WeightMin = 1;
        public int WeightMax = 10;

        public float NoItemChance = 0;
    }
}
