using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class OpenableItem : ItemExtension
    {
        public string TableName = "none";
        [XmlAttribute("Width")]
        public byte Width = 3;
        [XmlAttribute("Height")]
        public byte Height = 3;
        public List<string> UsedWhitelists;
    }
}
