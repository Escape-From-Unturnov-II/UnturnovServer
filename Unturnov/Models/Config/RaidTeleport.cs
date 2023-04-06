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
        public List<TeleportDestination> TeleportDestinations;

        public RaidTeleport()
        {

        }
        public RaidTeleport(ushort teleportFlag, List<TeleportDestination> teleportDestinations)
        {
            TeleportFlag = teleportFlag;
            TeleportDestinations = teleportDestinations;
        }
    }
}
