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
        public ushort CooldownMinFlag;
        public ushort CooldownSecFlag;
        public float CooldownInMin;

        public List<TeleportDestination> TeleportDestinations;

        public RaidTeleport()
        {

        }
        public RaidTeleport(ushort teleportFlag, ushort cooldownMinFlag, ushort cooldownSecFlag, List<TeleportDestination> teleportDestinations)
        {
            TeleportFlag = teleportFlag;
            CooldownInMin = cooldownMinFlag;
            CooldownSecFlag = cooldownSecFlag;
            TeleportDestinations = teleportDestinations;
        }
    }
}
