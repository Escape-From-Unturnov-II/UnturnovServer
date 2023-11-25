using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class HideoutConfig
    {
        public bool Debug;
        public uint SpawnedBarricadesPerFrame;
        public Notification_UI Notification_UI;
        public List<Position> HideoutPositions = new List<Position>();
    }
}
