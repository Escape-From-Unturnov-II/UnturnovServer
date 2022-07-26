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
        [XmlElement(ElementName = "Type")]
        public EAssetType type;
        public PlacementFoundation()
        {

        }
        public PlacementFoundation(ushort id, EAssetType type, string name = "")
        {
            Id = id;
            this.type = type;
            Name = name;
        }
    }
}
