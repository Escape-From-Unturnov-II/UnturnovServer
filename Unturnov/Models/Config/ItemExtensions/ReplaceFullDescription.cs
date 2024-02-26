using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class ReplaceFullDescription : ItemExtension 
    {
        [XmlIgnore]
        public byte RequiredAmount;
        public ItemExtension Result;
        public ReplaceFullDescription()
        {

        }
        public ReplaceFullDescription(ushort supplyId,ushort resultId, byte requiredAmount = 0)
        {
            Id = supplyId;
            RequiredAmount = requiredAmount;
            Result = new ItemExtension(resultId);
        }
    }
}
