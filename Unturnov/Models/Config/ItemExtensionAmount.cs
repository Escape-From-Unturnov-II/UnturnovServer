using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class ItemExtensionAmount : ItemExtension
    {
        public byte Amount = 1; 

        public ItemExtensionAmount()
        {

        }

        public ItemExtensionAmount(ushort itemId, string name = "", byte amount = 1)
        {
            Id = itemId;
            Name = name;
            Amount = amount;
        }
    }
}
