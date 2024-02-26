using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    internal class InventoryItemWrapper
    {
        internal ItemJar itemJar;
        internal ItemAsset asset => GetItemAsset();
        internal byte page;
        internal byte index;
        private ItemAsset _asset = null;
        internal InventoryItemWrapper(ItemJar itemJar, byte page, byte index)
        {
            this.itemJar = itemJar;
            this.page = page;
            this.index = index;
        }
        private ItemAsset GetItemAsset()
        {
            if (itemJar == null)
            {
                return null;
            }
            if (_asset == null)
            {
                _asset = Assets.find(EAssetType.ITEM, itemJar.item.id) as ItemAsset;
            }
            return _asset;
        } 
    }
}
