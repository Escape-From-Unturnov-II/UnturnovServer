using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class CombineDescription : ItemExtension
    {
        [XmlAttribute("RequiredAmount")]
        public ushort RequiredAmount;
        public ItemExtension Result;

        public CombineDescription()
        {

        }
        public CombineDescription(ushort supplyId, ushort requiredAmount, ushort resultId)
        {
            Id = supplyId;
            RequiredAmount = requiredAmount;
            Result = new ItemExtension(resultId);
        }
    }
}
