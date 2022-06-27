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
    //TODO: make scav mode unload resistent (flag change checks)
    //TODO: test stat save
    public class ScavRunControler
    {

        static Dictionary<ulong, StoredInventory> StoredInventories = new Dictionary<ulong, StoredInventory>();
        static Dictionary<ulong, PlayerStats> StoredStats = new Dictionary<ulong, PlayerStats>();
        private static Dictionary<ulong, ScavCooldownTimer> ScavCooldownTimers = new Dictionary<ulong, ScavCooldownTimer>();
        private static List<ulong> PlayerCommandChanges = new List<ulong>();
        private static bool isInit = false;

        public const string InventoryTableName = "ScavPlayerInventory";
        public const string PlayerStatsTableName = "PlayerStats";

        private const short scavReady = 0;
        private const short scavActive = 1;
        private const short scavCooldown = 2;

        public static void Init()
        {
            foreach (ScavKitTier tier in Unturnov.Conf.ScavKitTiers)
            {
                tier.localSet = new ScavSpawnTableSet(tier, Unturnov.Conf.ScavSpawnTables);
            }
            Unturnov.Database.CheckInventoryStorageSchema(InventoryTableName);
            Unturnov.Database.CheckPlayerStatsSchema(PlayerStatsTableName);
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
                
                switch (flag.value)
                {
                    // scav ready
                    case scavReady:
                        stopScavCooldown(player);
                        UnturnedChat.Say(player, Util.Translate("scav_ready"), UnityEngine.Color.green);
                        break;
                    // scav active
                    case scavActive:
                        stopScavCooldown(player);
                        if (PlayerCommandChanges.Contains(player.CSteamID.m_SteamID))
                        {
                            PlayerCommandChanges.Remove(player.CSteamID.m_SteamID);
                            break;
                        }
                        tryStart(player);
                        break;
                    // scav cooldown
                    case scavCooldown:
                        if (PlayerCommandChanges.Contains(player.CSteamID.m_SteamID))
                        {
                            PlayerCommandChanges.Remove(player.CSteamID.m_SteamID);
                            break;
                        }
                        tryStop(player);
                        break;
                    default:
                        Logger.LogError($"Error ScavRunControlFlag for {player.DisplayName} was set to an invalid value");
                        break;
                }
            }
        }
        internal static void OnPlayerConnected(UnturnedPlayer player)
        {
            if (!tryGetTier(player.Player.quests, out ScavKitTier tier))
            {
                Logger.LogError($"Error loading tier for player {player.DisplayName}");
            }
            if (isScavRunActive(player))
            {
                if(StoredInventories.TryGetValue(player.CSteamID.m_SteamID, out StoredInventory inventory))
                {
                    StoredInventories.Remove(player.CSteamID.m_SteamID);
                }
                
                inventory = Unturnov.Database.GetInventory(InventoryTableName, player.CSteamID.m_SteamID);
                StoredStats.Add(player.CSteamID.m_SteamID, Unturnov.Database.GetPlayerStats(PlayerStatsTableName, player.CSteamID.m_SteamID));
                StoredInventories.Add(player.CSteamID.m_SteamID, inventory);
                Unturnov.Database.RemoveInventory(InventoryTableName, player.CSteamID.m_SteamID);
            }
            else
            {
                startScavCooldown(player, tier);
            }
        }
        internal static void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (isScavRunActive(player) 
                && StoredInventories.TryGetValue(player.CSteamID.m_SteamID, out StoredInventory inventory)
                && StoredStats.TryGetValue(player.CSteamID.m_SteamID, out PlayerStats stats))
            {
                Unturnov.Database.SetInventory(InventoryTableName, player.CSteamID.m_SteamID, inventory);
                Unturnov.Database.SetPlayerStats(PlayerStatsTableName, player.CSteamID.m_SteamID, stats);
            }
            stopScavCooldown(player);
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
            if (!PlayerCommandChanges.Contains(player.CSteamID.m_SteamID))
            {
                PlayerCommandChanges.Add(player.CSteamID.m_SteamID);
            }
            ushort flag = Unturnov.Conf.ScavRunControlFlag;
            if (controlFlagCheck(flag))
            {
                player.Player.quests.sendSetFlag(flag, scavActive);
            }
            return tryStart(player);
        }
        private static bool tryStart(UnturnedPlayer player)
        {
            StoredInventory inventory = new StoredInventory();

            if (isInit
                && InventoryHelper.getClothingItems(player, ref inventory.clothing)
                && InventoryHelper.getInvItems(player, ref inventory.items)
                && !StoredInventories.ContainsKey(player.CSteamID.m_SteamID)
                && !StoredStats.ContainsKey(player.CSteamID.m_SteamID))
            {
                StoredInventories.Add(player.CSteamID.m_SteamID, inventory);
                StoredStats.Add(player.CSteamID.m_SteamID, PlayerStatManager.GetPlayerStats(player));

                InventoryHelper.clearAll(player);

                giveScavKit(player);

                Logger.Log($"{player.DisplayName} started a ScavRun");
                return true;
            }
            Logger.LogError($"Error starting ScavRun for {player.DisplayName}");
            return false;
        }
        internal static bool tryStopScavRun(UnturnedPlayer player)
        {
            if (!PlayerCommandChanges.Contains(player.CSteamID.m_SteamID))
            {
                PlayerCommandChanges.Add(player.CSteamID.m_SteamID);
            }
            ushort flag = Unturnov.Conf.ScavRunControlFlag;
            if (controlFlagCheck(flag))
            {
                player.Player.quests.sendSetFlag(flag, scavCooldown);
            }
            return tryStop(player);
        }
        private static bool tryStop(UnturnedPlayer player)
        {
            ulong steamId = player.CSteamID.m_SteamID;
            if (StoredInventories.TryGetValue(steamId, out StoredInventory storedInv) 
                && StoredStats.TryGetValue(steamId, out PlayerStats stats))
            {
                StoredInventories.Remove(steamId);
                StoredStats.Remove(steamId);

                InventoryHelper.clearAll(player);
                SecureCaseControler.resizeHands(player.Player);
                PlayerStatManager.SetPlayerStats(player, stats);

                foreach (KeyValuePair<InventoryHelper.StorageType, Item> entry in storedInv.clothing)
                {
                    player.Inventory.forceAddItem(entry.Value, true);
                }
                foreach (ItemJarWrapper itemJarWrap in storedInv.items)
                {
                    Unturnov.safeAddItem(player, itemJarWrap.itemJar.item, itemJarWrap.itemJar.x, itemJarWrap.itemJar.y, itemJarWrap.page, itemJarWrap.itemJar.rot);
                }

                Logger.Log($"{player.DisplayName} stopped his ScavRun");
                if (tryGetTier(player.Player.quests, out ScavKitTier tier))
                {
                    UnturnedChat.Say(player, Util.Translate("scav_cooldown", UnturnovCommands.formatTime(tier.Cooldown)), UnityEngine.Color.green);
                }
                return true;
            }
            Logger.LogError($"Error stopping ScavRun for {player.DisplayName}");
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
        internal static bool isScavRunActive(UnturnedPlayer player)
        {
            ushort flag = Unturnov.Conf.ScavRunControlFlag;
            return controlFlagCheck(flag) && player.Player.quests.getFlag(flag, out short value) && value == scavActive;
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
        
    }
}
