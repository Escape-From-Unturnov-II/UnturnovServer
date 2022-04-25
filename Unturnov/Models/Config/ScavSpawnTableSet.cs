using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class ScavSpawnTableSet
    {
        public SpawnTableExtension HatTable;
        public SpawnTableExtension GlassesTable;
        public SpawnTableExtension VestTable;
        public SpawnTableExtension BackpackTable;
        public SpawnTableExtension ShirtTable;
        public SpawnTableExtension PantsTable;

        public SpawnTableExtension GunTable;
        public SpawnTableExtension MedTable;
        public SpawnTableExtension SupplyTable;

        public ScavSpawnTableSet()
        {

        }
        public ScavSpawnTableSet(ScavKitTier tier, ScavSpawnTableSet globalSet)
        {
            HatTable = new SpawnTableExtension(tier.HatConfig, globalSet.HatTable);
            GlassesTable = new SpawnTableExtension(tier.GlassesConfig, globalSet.GlassesTable);
            VestTable = new SpawnTableExtension(tier.VestConfig, globalSet.VestTable);
            BackpackTable = new SpawnTableExtension(tier.BackpackConfig, globalSet.BackpackTable);
            ShirtTable = new SpawnTableExtension(tier.ShirtConfig, globalSet.ShirtTable);
            PantsTable = new SpawnTableExtension(tier.PantsConfig, globalSet.PantsTable);

            GunTable = new SpawnTableExtension(tier.GunConfig, globalSet.GunTable);
            MedTable = new SpawnTableExtension(tier.MedConfig, globalSet.MedTable);
            SupplyTable = new SpawnTableExtension(tier.SupplyConfig, globalSet.SupplyTable);
        }

    }
}
