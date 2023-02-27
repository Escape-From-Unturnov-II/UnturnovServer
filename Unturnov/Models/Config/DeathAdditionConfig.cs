using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class DeathAdditionConfig
    {
        public bool Debug = false;

        public bool KeepFood = true;
        public bool KeepWater = true;
        public bool KeepVirus = true;

        public bool DropHat = true;
        public bool DropMask = true;
        public bool DropGlasses = true;
        public bool DropBackpack = true;
        public bool DropVest = true;
        public bool DropShirt = true;
        public bool DropPants = true;

        public ushort DeathDropFlag;
        public List<DeathDrop> DeathDrops;
    }
}
