using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class PlacementFoundation : ItemExtension
    {
        [XmlAttribute("Type")]
        public EAssetType type;
        //TODO: Add Capacity
        /*
        [XmlAttribute("Capacity")]
        public uint capacity = 1;
        */
        public PlacementFoundation()
        {

        }
        public PlacementFoundation(ushort id, EAssetType type, string name = "") : base(id, name)
        {
            this.type = type;
        }
    }
}
