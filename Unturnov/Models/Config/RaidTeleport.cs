using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class RaidTeleport
    {
        public ushort TeleportFlag;
        public ushort CooldownQuestId;
        public ushort CooldownMinFlag;
        public ushort CooldownSecFlag;
        public float CooldownInMin;
        public TeleportDestination ExtractDestination;
        public List<TeleportDestination> TeleportDestinations;
        
        public RaidTeleport()
        {

        }
        public RaidTeleport(ushort teleportFlag, ushort cooldownQuestId, ushort cooldownMinFlag, ushort cooldownSecFlag, float cooldownInMin, TeleportDestination extractDestination ,List<TeleportDestination> teleportDestinations)
        {
            TeleportFlag = teleportFlag;
            CooldownQuestId = cooldownQuestId;
            CooldownSecFlag = cooldownSecFlag;
            CooldownInMin = cooldownInMin;
            ExtractDestination = extractDestination;
            TeleportDestinations = teleportDestinations;
        }
    }
}
