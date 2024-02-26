using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models.Config
{
    public class CaseSize
    {
        [XmlAttribute("Width")]
        public byte Width = 3;
        [XmlAttribute("Height")]
        public byte Height = 3;

        public CaseSize(byte width, byte height)
        {
            Width = width;
            Height = height;
        }
        public CaseSize()
        {

        }
    }
}
