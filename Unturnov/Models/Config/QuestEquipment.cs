using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class QuestEquipment
    {
        public string EquipmentIdentifier;
        public List<ItemExtension> Weapons;
        public List<ItemExtension> Hats;
        public List<ItemExtension> Vests;
        public List<ItemExtension> Backpack;
    }
}
