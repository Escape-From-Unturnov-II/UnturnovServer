using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models.Config
{
    public class PlayerKitConfig
    {
        public byte Health = 100;
        public byte Food = 100;
        public byte Water = 100;
        public byte Virus = 100;
        [XmlArrayItem(ElementName = "Item")]
        public List<ItemExtensionAmount> KitItems = new List<ItemExtensionAmount>();

        public PlayerKitConfig()
        {
        }
        public PlayerKitConfig(List<ItemExtensionAmount> kitItems, byte health = 100, byte food = 100, byte water = 100, byte virus = 100)
        {
            Health = health;
            Food = food;
            Water = water;
            Virus = virus;
            KitItems = kitItems;
        }
    }
}
