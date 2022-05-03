using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models.Config
{
    public class PlacementRestriction : ItemExtension
    {
        public List<string> ValidFoundationSetNames;
        [XmlIgnore]
        public Dictionary<ushort, PlacementFoundation> ValidItemFoundations = new Dictionary<ushort, PlacementFoundation>();
        [XmlIgnore]
        public Dictionary<ushort, PlacementFoundation> ValidObjectFoundations = new Dictionary<ushort, PlacementFoundation>();
    }
}
