using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models.Config
{
    public class PlacementRestriction
    {
        public ItemExtension RestrictedItem;
        public List<string> ValidFoundationSetNames;
        [XmlIgnore]
        public Dictionary<ushort,ItemExtension> ValidFoundations = new Dictionary<ushort, ItemExtension>();
    }
}
