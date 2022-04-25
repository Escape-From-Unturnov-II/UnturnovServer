using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Helper
{
    public class ScavRunController
    {
        static Dictionary<ulong, StoredInventory> StoredInventories = new Dictionary<ulong, StoredInventory>();
        internal static bool tryStartScavRun(UnturnedPlayer player)
        {
            StoredInventory inventory = new StoredInventory();

            if (InventoryHelper.GetClothingItems(player, ref inventory.clothing) && InventoryHelper.GetInvItems(player, ref inventory.items) && !StoredInventories.ContainsKey(player.CSteamID.m_SteamID))
            {
                StoredInventories.Add(player.CSteamID.m_SteamID, inventory);

                InventoryHelper.ClearAll(player);

                giveScavKit(player);

                return true;
            }

            return false;
        }

        internal static bool tryStopScavRun(UnturnedPlayer player)
        {
            if (StoredInventories.TryGetValue(player.CSteamID.m_SteamID, out StoredInventory storedInv))
            {
                StoredInventories.Remove(player.CSteamID.m_SteamID);

                InventoryHelper.ClearAll(player);
                foreach (KeyValuePair<InventoryHelper.StorageType, Item> entry in storedInv.clothing)
                {
                    player.Inventory.forceAddItem(entry.Value, true);
                }
                foreach (ItemJarWrapper itemJarWrap in storedInv.items)
                {
                    Unturnov.safeAddItem(player, itemJarWrap.itemJar.item, itemJarWrap.itemJar.x, itemJarWrap.itemJar.y, itemJarWrap.page, itemJarWrap.itemJar.rot);
                }
                return true;
               
            }
            return false;
        }

        internal static void giveScavKit(UnturnedPlayer player) 
        {
            foreach (ItemExtension itemEx in Unturnov.Conf.ScavSpawnTables[0].Clothing)
            {
                Item item = new Item(itemEx.Id, true);
                player.Inventory.forceAddItem(item, true);
            }
            foreach (ItemExtension itemEx in Unturnov.Conf.ScavSpawnTables[0].Items)
            {
                Item item = new Item(itemEx.Id, true);
                player.Inventory.forceAddItem(item, true);
            }

        }

        internal class StoredInventory
        {
            internal List<KeyValuePair<InventoryHelper.StorageType, Item>> clothing;
            internal List<ItemJarWrapper> items;

            internal StoredInventory()
            {
                clothing = new List<KeyValuePair<InventoryHelper.StorageType, Item>>();
                items = new List<ItemJarWrapper>();
            }


        }
    }
}
