using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class DeathDrop : ItemExtension
    {
        [XmlAttribute("RequiredFalgValue")]
        public short RequiredFalgValue;
    }
}
