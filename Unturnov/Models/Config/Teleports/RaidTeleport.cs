using SpeedMann.Unturnov.Models.Config.QuestExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class RaidTeleport
    {
        public string RaidName = "MapName";
        public ushort TeleportFlag;
        public QuestCooldown QuestCooldown = new QuestCooldown();
        public float CooldownInMin;
        public TeleportDestination ExtractDestination;
        public List<TeleportDestination> TeleportDestinations;
        
        public RaidTeleport()
        {

        }
        public RaidTeleport(string raidName, ushort teleportFlag, QuestCooldown questCooldown, float cooldownInMin, TeleportDestination extractDestination ,List<TeleportDestination> teleportDestinations)
        {
            RaidName = raidName;
            TeleportFlag = teleportFlag;
            if (questCooldown != null)
            {
                QuestCooldown = questCooldown;
            }
            CooldownInMin = cooldownInMin;
            ExtractDestination = extractDestination;
            TeleportDestinations = teleportDestinations;
        }
    }
}
