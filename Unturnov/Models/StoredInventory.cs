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

        public StoredInventory()
        {
            clothing = new List<KeyValuePair<InventoryHelper.StorageType, Item>>();
            items = new List<ItemJarWrapper>();
        }
    }
}
