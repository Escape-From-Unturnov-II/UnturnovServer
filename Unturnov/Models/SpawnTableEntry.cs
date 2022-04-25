using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class SpawnTableEntry : ItemExtension
    {
        public int weight = 0;
        [XmlIgnore]
        public float chance = 0;

        public SpawnTableEntry()
        {

        }
        public SpawnTableEntry(ushort itemId, int weight, float chance = 0)
        {
            this.Id = itemId;
            this.weight = weight;
            this.chance = chance;
        }
    }
}
