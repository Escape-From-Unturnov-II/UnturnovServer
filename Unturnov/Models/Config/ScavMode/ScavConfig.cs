using SpeedMann.Unturnov.Models.Config.QuestExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class ScavConfig
    {
        public ushort ScavKitTierFlag;
        public ushort ScavRunControlFlag;
        public QuestCooldown QuestCooldown = new QuestCooldown();
        public List<ScavKitTier> ScavKitTiers = new List<ScavKitTier>();
        public ScavSpawnTableSet ScavSpawnTables = new ScavSpawnTableSet();

        public ScavConfig()
        {

        }
        public ScavConfig(ushort controllerFlag, ushort tierFlag, QuestCooldown questCooldown, List<ScavKitTier> scavKits, ScavSpawnTableSet scavSpawnTables)
        {
            ScavRunControlFlag = controllerFlag;
            ScavKitTierFlag = tierFlag;
            if(questCooldown != null)
            {
                QuestCooldown = questCooldown;
            }
            ScavKitTiers = scavKits;
            ScavSpawnTables = scavSpawnTables;
        }
    }
}
