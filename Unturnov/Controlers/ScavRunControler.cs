using Google.Protobuf.WellKnownTypes;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Commands;
using SpeedMann.Unturnov.Controlers;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    //TODO: rework with coroutine and quest (similar to teleports)
    public class ScavRunControler
    {

        static Dictionary<ulong, StoredInventory> StoredInventories = new Dictionary<ulong, StoredInventory>();
        static Dictionary<ulong, PlayerStats> StoredStats = new Dictionary<ulong, PlayerStats>();

        private static ScavConfig Conf;
        private static bool isInit = false;

        public const string InventoryTableName = "ScavPlayerInventory";
        public const string PlayerStatsTableName = "PlayerStats";

        private const short scavReady = 0;
        private const short scavRequestStart = 1;
        private const short scavActive = 2;
        private const short scavRequestEnd = 3;
        private const short scavCooldown = 4;

        public static void Init(ScavConfig scavConfig)
        {
            Conf = scavConfig;
            foreach (ScavKitTier tier in Conf.ScavKitTiers)
            {
                tier.localSet = new ScavSpawnTableSet(tier, Conf.ScavSpawnTables);
            }
            Unturnov.Database.CheckInventoryStorageSchema(InventoryTableName);
            Unturnov.Database.CheckPlayerStatsSchema(PlayerStatsTableName);

            // reload checks
            foreach (SteamPlayer player in Provider.clients)
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player.player);
                OnPlayerConnected(uPlayer);
            }

            isInit = true;
        }
        public static void Cleanup()
        {
            foreach (KeyValuePair<ulong, PlayerStats> statsPair in StoredStats)
            {
                Unturnov.Database.SetPlayerStats(PlayerStatsTableName, statsPair.Key, statsPair.Value);
            }
            foreach (KeyValuePair<ulong, StoredInventory> inventoryPair in StoredInventories)
            {
                Unturnov.Database.SetInventory(InventoryTableName, inventoryPair.Key, inventoryPair.Value);
            }
        }

        internal static void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            if (flag.id == Conf.ScavRunControlFlag && tryGetTier(quests, out ScavKitTier tier))
            {
                UnturnedPlayer player = UnturnedPlayer.FromPlayer(quests.player);
                
                switch (flag.value)
                {
                    case scavReady:
                        UnturnedChat.Say(player, Util.Translate("scav_ready"), Color.green);
                        break;
                    case scavRequestStart:
                        tryStartScavRun(player);
                        break;
                    case scavRequestEnd:
                        tryStopScavRun(player);
                        break;
                    case scavActive:
                    case scavCooldown:
                        break;
                    default:
                        Logger.LogError($"Error ScavRunControlFlag for {player.DisplayName} was set to an invalid value");
                        break;
                }
            }
        }
        internal static void OnPlayerConnected(UnturnedPlayer player)
        {
            ushort flag = Conf.ScavRunControlFlag;
            if (!controlFlagCheck(flag) || !player.Player.quests.getFlag(flag, out short value))
            {
                return;
            }
            if (!tryGetTier(player.Player.quests, out ScavKitTier tier))
            {
                Logger.LogError($"Error loading tier for player {player.DisplayName}");
                return;
            }

            bool isScavActive = false;
            switch (value)
            {
                case scavReady:
                    break;
                case scavActive:
                    isScavActive = true;
                    break;
                case scavCooldown:
                    TryRestoreScavCooldown(player);               
                    break;
                default:
                    Logger.LogError($"Player {player.CSteamID} joined with invalid scav flag {flag} value {value}");
                    player.Player.quests.sendSetFlag(flag, scavReady);
                    break;
            }

            if (isScavActive)
            {
                if(StoredStats.TryGetValue(player.CSteamID.m_SteamID, out PlayerStats stats))
                {
                    StoredStats.Remove(player.CSteamID.m_SteamID);
                }
                if (StoredInventories.TryGetValue(player.CSteamID.m_SteamID, out StoredInventory inventory))
                {
                    StoredInventories.Remove(player.CSteamID.m_SteamID);
                }

                inventory = Unturnov.Database.GetInventory(InventoryTableName, player.CSteamID.m_SteamID);
                stats = Unturnov.Database.GetPlayerStats(PlayerStatsTableName, player.CSteamID.m_SteamID);
                StoredStats.Add(player.CSteamID.m_SteamID, stats);
                StoredInventories.Add(player.CSteamID.m_SteamID, inventory);
                Unturnov.Database.RemoveInventory(InventoryTableName, player.CSteamID.m_SteamID);
                return;
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
        }
        internal static void OnPlayerRevived(PlayerLife playerLife)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(playerLife.player);
            if (isScavRunActive(uPlayer))
            {
                if (!tryStopScavRun(uPlayer))
                {
                    Logger.LogError($"Could not stop scav run of player {uPlayer.CSteamID} on revive");
                }
            }
        }
        internal static bool tryStartScavRun(UnturnedPlayer player)
        {
            ushort flag = Conf.ScavRunControlFlag;
            if (controlFlagCheck(flag))
            {
                Unturnov.ChangeFlagDelayed(player.Player, flag, scavActive);
            }
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
                PlayerStatManager.SetPlayerStats(player, new PlayerStats());

                giveScavKit(player);

                Logger.Log($"{player.DisplayName} started a ScavRun");
                return true;
            }
            Logger.LogError($"Error starting ScavRun for {player.DisplayName}");
            return false;
        }

        internal static bool tryStopScavRun(UnturnedPlayer player)
        {
            ushort flag = Conf.ScavRunControlFlag;

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
                    InventoryHelper.forceAddItem(player, itemJarWrap.itemJar.item, itemJarWrap.itemJar.x, itemJarWrap.itemJar.y, itemJarWrap.page, itemJarWrap.itemJar.rot);
                }

                Logger.Log($"{player.DisplayName} stopped his ScavRun");
                if (tryGetTier(player.Player.quests, out ScavKitTier tier))
                {
                    UnturnedChat.Say(player, Util.Translate("scav_cooldown", ScavCommands.formatTime(tier.CooldownInMin*60)), Color.green);
                    TryStartScavCooldown(player, tier); 
                    if (controlFlagCheck(flag))
                    {
                        Unturnov.ChangeFlagDelayed(player.Player, flag, scavCooldown);
                    }
                }
                return true;
            }
            Logger.LogError($"Error stopping ScavRun for {player.DisplayName}");
            return false;
        }

        internal static void giveScavKit(UnturnedPlayer player) 
        {
            if (Conf.ScavKitTiers.Count > 0)
            {
                // check tier flag
                ScavKitTier tier = null;
                if (Conf.ScavKitTierFlag != 0 && player.Player.quests.getFlag(Conf.ScavKitTierFlag, out short flagValue))
                {
                    tier = Conf.ScavKitTiers.Find(x => x.RequiredFalgValue == flagValue);
                }
                // default to index 0
                if(tier == null)
                {
                    tier = Conf.ScavKitTiers[0];
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
        internal static void giveScavGuns(UnturnedPlayer player, KitTierGunEntry entry, SpawnTableExtension table)
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
                    Item item = null;
                    SpawnTableGunEntry gunEntry = table.getRandomEntry() as SpawnTableGunEntry;
                    if(gunEntry != null)
                    {
                        item = new Item(gunEntry.Id, true);
                    }
                    if (item == null)
                    {
                        Logger.LogError($"Error in Scav Spawn table, could not give gun");
                        continue;
                    }
                    player.Inventory.forceAddItem(item, true);
                }
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
                    ushort itemId = table.getRandomItem();
                    Item item = new Item(itemId, true);
                    if (item == null || itemId == 0)
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
            ushort flag = Conf.ScavRunControlFlag;
            if (controlFlagCheck(flag) && player.Player.quests.getFlag(flag, out short value))
            {
                switch (value)
                {
                    case scavReady:
                        state = "ready";
                        break;
                    case scavRequestStart:
                        state = "startRequested";
                        break;
                    case scavActive:
                        state = "active";
                        break;
                    case scavRequestEnd:
                        state = "stopRequested";
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
            if(Conf.ScavKitTiers.Count <= 0)
            {
                tier = null;
                return false;
            }

            ushort tierFlag = Conf.ScavKitTierFlag;
            short scavTierIndex;
            if (tierFlag == 0 || !quests.getFlag(tierFlag, out scavTierIndex) || scavTierIndex < 0)
            {
                scavTierIndex = 0;
            }
            if (scavTierIndex >= Conf.ScavKitTiers.Count)
            {
                scavTierIndex = (short)(Conf.ScavKitTiers.Count - 1);
            }
            tier = Conf.ScavKitTiers[scavTierIndex];
            return true;
        }
        internal static bool isScavRunActive(UnturnedPlayer player)
        {
            ushort flag = Conf.ScavRunControlFlag;
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
        private static bool TryRestoreScavCooldown(UnturnedPlayer player)
        {
            ushort flag = Conf.ScavRunControlFlag;
            if (!controlFlagCheck(flag))
            {
                return false;
            }

            player.Player.StartCoroutine(TryStartScavCooldownDelayed(player, 0, flag, true));
            return true;
        }
        private static bool TryStartScavCooldown(UnturnedPlayer player, ScavKitTier tier)
        {
            ushort flag = Conf.ScavRunControlFlag;
            if (!controlFlagCheck(flag))
            {
                return false;
            }
            player.Player.StartCoroutine(TryStartScavCooldownDelayed(player, tier.CooldownInMin, flag));
            return true;
        }
        private static IEnumerator TryStartScavCooldownDelayed(UnturnedPlayer player, float cooldown, ushort flag, bool restore = false)
        {
            yield return null;
            QuestExtensionControler.TryStartQuestBasedCooldown(player.Player,
                Conf.QuestCooldown,
                cooldown,
                flag,
                scavCooldown,
                scavReady,
                restore);
        }
        #endregion

    }
}
