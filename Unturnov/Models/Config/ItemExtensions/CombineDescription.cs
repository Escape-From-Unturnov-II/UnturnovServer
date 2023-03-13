using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class CombineDescription : ItemExtension
    {
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
