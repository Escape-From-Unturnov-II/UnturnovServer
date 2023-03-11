using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class PlayerKitConfig
    {
        public byte Health = 100;
        public byte Food = 100;
        public byte Water = 100;
        public byte Virus = 100;

        public List<ItemExtensionAmount> KitItems = new List<ItemExtensionAmount>();

        public PlayerKitConfig()
        {
        }
        public PlayerKitConfig(byte health, byte food, byte water, byte virus, List<ItemExtensionAmount> kitItems)
        {
            Health = health;
            Food = food;
            Water = water;
            Virus = virus;
            KitItems = kitItems;
        }
    }
}
