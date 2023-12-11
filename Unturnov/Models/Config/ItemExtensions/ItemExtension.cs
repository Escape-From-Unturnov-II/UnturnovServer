using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class ItemExtension
    {
        [XmlAttribute("Id")]
        public ushort Id;
        [XmlAttribute("Name")]
        public string Name;

        public ItemExtension()
        {

        }

        public ItemExtension( ushort itemId, string name = "")
        {
            Id = itemId;
            Name = name;
        }
    }
}
