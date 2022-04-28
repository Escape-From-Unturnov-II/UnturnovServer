using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    //TODO: save inventory to db
    //TODO: fix relogging resetting hand size (SecureCase)
    public class ScavRunControler
    {

        static Dictionary<ulong, StoredInventory> StoredInventories = new Dictionary<ulong, StoredInventory>();
        private static Dictionary<ulong, ScavCooldownTimer> ScavCooldownTimers = new Dictionary<ulong, ScavCooldownTimer>();
        private static bool isInit = false;

        private const short scavReady = 0;
        private const short scavActive = 1;
        private const short scavCooldown = 2;

       

        public static void Init()
        {
            foreach (ScavKitTier tier in Unturnov.Conf.ScavKitTiers)
            {
                tier.localSet = new ScavSpawnTableSet(tier, Unturnov.Conf.ScavSpawnTables);
            }
            isInit = true;
        }
        public static void Cleanup()
        {
            foreach (KeyValuePair<ulong, ScavCooldownTimer> entry in ScavCooldownTimers)
            {
                entry.Value.Stop();
            }
            ScavCooldownTimers = new Dictionary<ulong, ScavCooldownTimer>();
        }

        internal static void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            if (flag.id == Unturnov.Conf.ScavRunControlFlag && tryGetTier(quests, out ScavKitTier tier))
            {
                UnturnedPlayer player = UnturnedPlayer.FromPlayer(quests.player);
                stopScavCooldown(player);
                switch (flag.value)
                {
                    // scav ready
                    case scavReady:
                        UnturnedChat.Say(player, Util.Translate("scav_ready"), UnityEngine.Color.green);
                        break;
                    // scav active
                    case scavActive:
                        if (!tryStartScavRun(player))
                        {
                            Logger.LogError($"Error starting ScavRun for {player.DisplayName}");
                        }
                        break;
                    // scav cooldown
                    case scavCooldown:
                        if (!tryStopScavRun(player))
                        {
                            Logger.LogError($"Error stopping ScavRun for {player.DisplayName}");
                        }
                        startScavCooldown(player, tier);
                        break;
                }
            }
        }

        internal static void startScavCooldown(UnturnedPlayer player, ScavKitTier tier)
        {
            ushort flag = Unturnov.Conf.ScavRunControlFlag;
            if (controlFlagCheck(flag))
            {
                ScavCooldownTimer timer;
                if (ScavCooldownTimers.TryGetValue(player.CSteamID.m_SteamID, out timer))
                {
                    timer.Stop();
                    ScavCooldownTimers.Remove(player.CSteamID.m_SteamID);
                }
                timer = new ScavCooldownTimer(tier, player.Player.quests);
                ScavCooldownTimers.Add(player.CSteamID.m_SteamID, timer);
                timer.Elapsed += queueEnableScavMode;

                timer.Start();
            }
        }

        internal static void stopScavCooldown(UnturnedPlayer player)
        {
            if (ScavCooldownTimers.TryGetValue(player.CSteamID.m_SteamID, out ScavCooldownTimer timer))
            {
                timer.Stop();
                ScavCooldownTimers.Remove(player.CSteamID.m_SteamID);
            }
        }

        internal static void queueEnableScavMode(PlayerQuests quests)
        {           
            ushort flag = Unturnov.Conf.ScavRunControlFlag;
            if (controlFlagCheck(flag))
            {
                Unturnov.MainThreadQueue.Add(new SetFalgQueueEntry(quests, flag, scavReady));
            }
        }

        internal static bool tryStartScavRun(UnturnedPlayer player)
        {
            StoredInventory inventory = new StoredInventory(player.Inventory.items[2].width, player.Inventory.items[2].height);

            if (isInit 
                && InventoryHelper.GetClothingItems(player, ref inventory.clothing) 
                && InventoryHelper.GetInvItems(player, ref inventory.items) 
                && !StoredInventories.ContainsKey(player.CSteamID.m_SteamID))
            {
                StoredInventories.Add(player.CSteamID.m_SteamID, inventory);

                InventoryHelper.ClearAll(player);

                giveScavKit(player);

                Logger.Log($"{player.DisplayName} started a ScavRun");
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
                player.Inventory.items[2].resize(storedInv.handWidth, storedInv.handHeight);

                foreach (KeyValuePair<InventoryHelper.StorageType, Item> entry in storedInv.clothing)
                {
                    player.Inventory.forceAddItem(entry.Value, true);
                }
                foreach (ItemJarWrapper itemJarWrap in storedInv.items)
                {
                    Unturnov.safeAddItem(player, itemJarWrap.itemJar.item, itemJarWrap.itemJar.x, itemJarWrap.itemJar.y, itemJarWrap.page, itemJarWrap.itemJar.rot);
                }

                
                ushort flag = Unturnov.Conf.ScavRunControlFlag;
                if (controlFlagCheck(flag))
                {
                    player.Player.quests.sendSetFlag(flag, scavCooldown);
                }
                Logger.Log($"{player.DisplayName} stopped his ScavRun");
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

                player.Inventory.items[2].resize(tier.HandWidth, tier.HandHeight);

                giveScavItems(player, tier.GlassesConfig, tier.localSet.GlassesTable);
                giveScavItems(player, tier.MaskConfig, tier.localSet.MaskTable);
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

        #region Helper Functions
        public static bool tryGetStateName(UnturnedPlayer player, out string state)
        {
            state = "unknown";
            ushort flag = Unturnov.Conf.ScavRunControlFlag;
            if (controlFlagCheck(flag) && player.Player.quests.getFlag(flag, out short value))
            {
                switch (value)
                {
                    case scavReady:
                        state = "ready";
                        break;
                    case scavActive:
                        state = "active";
                        break;
                    case scavCooldown:
                        state = "cooldown";
                        break;
                }
                return true;
            }
            return false;
        }
        internal static bool tryGetTier(PlayerQuests quests, out ScavKitTier tier)
        {
            if(Unturnov.Conf.ScavKitTiers.Count <= 0)
            {
                tier = null;
                return false;
            }

            ushort tierFlag = Unturnov.Conf.ScavKitTierFlag;
            short scavTierIndex;
            if (tierFlag == 0 || !quests.getFlag(tierFlag, out scavTierIndex) || scavTierIndex < 0)
            {
                scavTierIndex = 0;
            }
            if (scavTierIndex >= Unturnov.Conf.ScavKitTiers.Count)
            {
                scavTierIndex = (short)(Unturnov.Conf.ScavKitTiers.Count - 1);
            }
            tier = Unturnov.Conf.ScavKitTiers[scavTierIndex];
            return true;
        }

        internal static bool controlFlagCheck(ushort flag)
        {
            if (flag == 0)
            {
                Logger.LogError("ScavRunControlFlag can not be 0");
                return false;
            }
            return true;
        }
        #endregion
        internal class StoredInventory
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

        }
    }
}
