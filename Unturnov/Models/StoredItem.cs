using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class StoredItem
    {
        public ushort id;
        public byte amount;
        public byte quality;
        public byte[] state;

        public byte x;
        public byte y;
        public byte rot;
        public StoredItem(Item item, byte x, byte y, byte rot)
        {
            id = item.id;
            amount = item.amount;
            quality = item.durability;
            state = item.metadata;
            this.x = x;
            this.y = y;
            this.rot = rot;
        }
        public StoredItem()
        {

        }
    }
}
