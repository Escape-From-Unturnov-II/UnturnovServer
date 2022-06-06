using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class StoredInventory
    {
        internal List<KeyValuePair<InventoryHelper.StorageType, Item>> clothing;
        internal List<ItemJarWrapper> items;
        internal byte handWidth;
        internal byte handHeight;

        internal StoredInventory(byte handWidth, byte handHeight)
        {
            this.handWidth = handWidth;
            this.handHeight = handHeight;

            clothing = new List<KeyValuePair<InventoryHelper.StorageType, Item>>();
            items = new List<ItemJarWrapper>();
        }
        public StoredInventory()
        {
            this.handWidth = 0;
            this.handHeight = 0;

            clothing = new List<KeyValuePair<InventoryHelper.StorageType, Item>>();
            items = new List<ItemJarWrapper>();
        }
    }
}
