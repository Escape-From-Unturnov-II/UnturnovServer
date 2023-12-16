using SpeedMann.Unturnov.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class ScavKitTier
    {
        public short RequiredFalgValue;
        public float CooldownInMin;
        public byte HandWidth;
        public byte HandHeight;

        public KitTierEntry HatConfig;
        public KitTierEntry GlassesConfig;
        public KitTierEntry MaskConfig;
        public KitTierEntry VestConfig;
        public KitTierEntry BackpackConfig;
        public KitTierEntry ShirtConfig;
        public KitTierEntry PantsConfig;

        public KitTierEntry GunConfig;
        public KitTierEntry MedConfig;
        public KitTierEntry SupplyConfig;

        [XmlIgnore]
        public ScavSpawnTableSet localSet = null;
    }
}
