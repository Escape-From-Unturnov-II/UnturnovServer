using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class TeleportConfig
    {
        public bool Debug;
        public float SquadTeleportRadius;
        public ushort HideoutTeleportFlag;
        public List<RaidTeleport> RaidTeleports;
        public TeleportConfig()
        {

        }
        public TeleportConfig(bool debug, float squadTeleportRadius, ushort hideoutTeleportFlag, List<RaidTeleport> raidTeleports)
        {
            Debug = debug;
            SquadTeleportRadius = squadTeleportRadius;
            HideoutTeleportFlag = hideoutTeleportFlag;
            RaidTeleports = raidTeleports;
        }
    }
}
