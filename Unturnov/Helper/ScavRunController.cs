using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    public class ScavRunController
    {
        static Dictionary<ulong, StoredInventory> StoredInventories = new Dictionary<ulong, StoredInventory>();
        static bool isInit = false;
        public static void Init()
        {
            foreach (ScavKitTier tier in Unturnov.Conf.ScavKitTiers)
            {
                tier.localSet = new ScavSpawnTableSet(tier, Unturnov.Conf.ScavSpawnTables);
            }
            isInit = true;
        }

        internal static bool tryStartScavRun(UnturnedPlayer player)
        {
            StoredInventory inventory = new StoredInventory();

            if (isInit 
                && InventoryHelper.GetClothingItems(player, ref inventory.clothing) 
                && InventoryHelper.GetInvItems(player, ref inventory.items) 
                && !StoredInventories.ContainsKey(player.CSteamID.m_SteamID))
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
            if (Unturnov.Conf.ScavKitTiers.Count > 0)
            {
                // check tier flag
                ScavKitTier tier = null;
                if (Unturnov.Conf.ScavKitTierFlag != 0 && player.Player.quests.getFlag(Unturnov.Conf.ScavKitTierFlag, out short flagValue))
                {
                    tier = Unturnov.Conf.ScavKitTiers.Find(x => x.RequiredFalgValue == flagValue);
                }
                // default to index 0
                if(tier == null)
                {
                    tier = Unturnov.Conf.ScavKitTiers[0];
                }

                giveScavItems(player, tier.GlassesConfig, tier.localSet.GlassesTable);
                giveScavItems(player, tier.HatConfig, tier.localSet.HatTable);
                giveScavItems(player, tier.BackpackConfig, tier.localSet.BackpackTable);
                giveScavItems(player, tier.VestConfig, tier.localSet.VestTable);
                giveScavItems(player, tier.ShirtConfig, tier.localSet.ShirtTable);
                giveScavItems(player, tier.PantsConfig, tier.localSet.PantsTable);

                giveScavItems(player, tier.GunConfig, tier.localSet.GunTable);
                giveScavItems(player, tier.MedConfig, tier.localSet.MedTable);
                giveScavItems(player, tier.SupplyConfig, tier.localSet.SupplyTable);
            }
        }
        internal static void giveScavItems(UnturnedPlayer player, KitTierEntry entry, SpawnTableExtension table)
        {
            if (table.Items.Count > 0)
            {
                int count = entry.CountMax > entry.CountMin ? entry.CountMax : entry.CountMin;

                for (int i = 0; i < count; i++)
                {
                    if (i >= entry.CountMin && UnityEngine.Random.value < entry.NoItemChance)
                    {
                        continue;
                    }
                    ushort itemId = table.getItem();
                    Item item = new Item(itemId, true);
                    if (item == null)
                    {
                        Logger.LogError($"Error in Scav Spawn table, invalid ItemId: {itemId}");
                        continue;
                    }
                    player.Inventory.forceAddItem(item, true);
                }
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
