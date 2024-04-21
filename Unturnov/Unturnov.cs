using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpeedMann.Unturnov.Controlers;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using Logger = Rocket.Core.Logging.Logger;
using System.Collections;


namespace SpeedMann.Unturnov
{
    public class Unturnov : RocketPlugin<UnturnovConfiguration>
    {
        public static Unturnov Inst;
        public static UnturnovConfiguration Conf;
        public static DatabaseManager Database;
        public static bool ModsLoaded = false;

        internal static List<CSteamID> ReplaceBypass;
        internal static List<MainQueueEntry> MainThreadQueue = new List<MainQueueEntry>();

        private int updateDelay = 30;
        private int frame = 0;

        private uint oldBedTimer;

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                { "scav_ready", "Scav run is ready" },
                { "scav_cooldown", "Scav mode is ready in {0}"},
                { "raid_ready", "Raid {0} is ready"},
                { "raid_cooldown", "Raid {0} is on cooldown for {1}"},
                { "container_item_restricted", "You are not allowed to store this {0} in the secure container!" },
                { "hideout_out_of_bounds", "You can only build in your hideout!" },
                { "hideout_not_ready", "Your hideout is not ready yet!" },
                { "no_hideout", "You do not have a hideout!" },
            };

        #region Load
        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            Database = new DatabaseManager();
            ReplaceBypass = new List<CSteamID>();

            // force set bed timer
            oldBedTimer = Provider.modeConfigData.Gameplay.Timer_Home;
            Provider.modeConfigData.Gameplay.Timer_Home = Conf.BedTimer;

            UnturnedPrivateFields.Init();
            UnturnedPatches.Init();
            ScavRunControler.Init(Conf.ScavConfig);
            TeleportControler.Init(Conf.TeleportConfig);
            SecureCaseControler.Init(Conf.SecureCaseConfig);
            PlacementRestrictionControler.Init(Conf.PlacementRestrictionConfig);
            OpenableItemsControler.Init();
            QuestExtensionControler.Init();
            DeathAdditionsControler.Init(Conf.DeathDropConfig);
            WeaponModdingControler.Init(Conf.GunModdingResults);
            JsonManager.Init(Directory);


            Conf.updateConfig();

            UnturnedPatches.OnPreTryAddItemAuto += OnTryAddItem;

            UnturnedPatches.OnSendSetFlag += OnFlagChanged;

            UnturnedPatches.OnPreEquipmentUpdateState += OnEquipmentStateUpdate;
            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested += OnCraft;
            UnturnedPatches.OnPreForceGiveItem += OnPreForceGiveItem;

            BarricadeManager.onDeployBarricadeRequested += OnBarricadeDeploy;
            BarricadeManager.onBarricadeSpawned += OnBarricadeSpawned;
            BarricadeDrop.OnSalvageRequested_Global += OnBarricadeSalvageRequest;
            UnturnedPatches.OnPreDestroyBarricade += OnBarricadeDestroy;

            UnturnedPatches.OnPreBarricadeStorageRequest += OnBarricadeStorageRequest;
            UnturnedPatches.OnPostGetInput += OnGetInput;

            StructureManager.onStructureSpawned += OnStructureSpawned;

            UnturnedPatches.OnPrePlayerDead += OnPrePlayerDead;
            UnturnedPatches.OnPostPlayerRevive += OnPlayerRevived;

            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            DamageTool.damageAnimalRequested += OnAnimalDamage;
            DamageTool.damageZombieRequested += OnZombieDamage;
            ResourceManager.onDamageResourceRequested += OnResourceDamage;
            Player.onPlayerStatIncremented += OnStatIncremented;

            UnturnedPatches.onZombieDeath += OnZombieDeath;
            UnturnedPatches.onAnimalDeath += OnAnimalDeath;

            UnturnedPatches.OnPrePlayerDraggedItem += OnItemDragged;
            UnturnedPatches.OnPrePlayerSwappedItem += OnItemSwapped;
            UnturnedPatches.OnPrePlayerAddItem += OnPreItemAdded;
            ItemManager.onTakeItemRequested += OnTakeItem;

            UnturnedPatches.OnPreInteractabilityCondition += OnInteractableConditionCheck;
            PlayerEquipment.OnUseableChanged_Global += OnEquipmentChanged;
            UseableThrowable.onThrowableSpawned += OnThrowableSpawned;

            PlayerEquipment.OnInspectingUseable_Global += OnInspect;

            UnturnedPatches.OnPreDisconnectSave += OnPreDisconnectSave;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            U.Events.OnPlayerConnected += OnPlayerConnected;

            Level.onPreLevelLoaded += OnPreLevelLoaded;

            if (Level.isLoaded)
            {
                OnPreLevelLoaded(0);
            }
            printPluginInfo();

            List<SteamPlayer> players = Provider.clients;
            foreach (SteamPlayer player in players)
            {
                OnPlayerConnectedInner(UnturnedPlayer.FromSteamPlayer(player), true);
            }
        }

        protected override void Unload()
        {

            ScavRunControler.Cleanup();
            TeleportControler.Cleanup();
            QuestExtensionControler.Cleanup();
            HideoutControler.Cleanup();
            PlacementRestrictionControler.Cleanup();
            AirdropControler.Cleaup();

            Provider.modeConfigData.Gameplay.Timer_Home = oldBedTimer;

            UnturnedPatches.OnPreTryAddItemAuto -= OnTryAddItem;

            UnturnedPatches.OnSendSetFlag -= OnFlagChanged;

            UnturnedPatches.OnPreEquipmentUpdateState -= OnEquipmentStateUpdate;
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested -= OnCraft;
            UnturnedPatches.OnPreForceGiveItem -= OnPreForceGiveItem;

            BarricadeManager.onDeployBarricadeRequested -= OnBarricadeDeploy;
            BarricadeManager.onBarricadeSpawned -= OnBarricadeSpawned;
            BarricadeDrop.OnSalvageRequested_Global -= OnBarricadeSalvageRequest;
            UnturnedPatches.OnPreDestroyBarricade -= OnBarricadeDestroy;

            UnturnedPatches.OnPostGetInput -= OnGetInput;
            UnturnedPatches.OnPreBarricadeStorageRequest -= OnBarricadeStorageRequest;
            StructureManager.onStructureSpawned -= OnStructureSpawned;

            UnturnedPatches.OnPrePlayerDead -= OnPrePlayerDead;
            UnturnedPatches.OnPostPlayerRevive -= OnPlayerRevived;

            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            DamageTool.damageAnimalRequested -= OnAnimalDamage;
            DamageTool.damageZombieRequested -= OnZombieDamage;
            ResourceManager.onDamageResourceRequested -= OnResourceDamage;
            Player.onPlayerStatIncremented -= OnStatIncremented;

            UnturnedPatches.onZombieDeath -= OnZombieDeath;
            UnturnedPatches.onAnimalDeath -= OnAnimalDeath;

            UnturnedPatches.OnPrePlayerDraggedItem -= OnItemDragged;
            UnturnedPatches.OnPrePlayerSwappedItem -= OnItemSwapped;
            UnturnedPatches.OnPrePlayerAddItem -= OnPreItemAdded;
            ItemManager.onTakeItemRequested -= OnTakeItem;

            UnturnedPatches.OnPreInteractabilityCondition -= OnInteractableConditionCheck;
            PlayerEquipment.OnUseableChanged_Global -= OnEquipmentChanged;
            PlayerEquipment.OnInspectingUseable_Global -= OnInspect;
            UseableThrowable.onThrowableSpawned -= OnThrowableSpawned;

            UnturnedPatches.OnPreDisconnectSave -= OnPreDisconnectSave;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            U.Events.OnPlayerConnected -= OnPlayerConnected;

            Level.onPreLevelLoaded -= OnPreLevelLoaded;


            UnturnedPatches.Cleanup();

            Inst = null;
            Conf = null;
        }
        #endregion
        private void OnPreLevelLoaded(int level)
        {
            Conf.addNames();
            UnloadMagControler.Init(Conf.UnloadMagBlueprints);
            HideoutControler.Init(Conf.HideoutConfig);
            AirdropControler.Init(Conf.AirdropSignals);
            ItemStackController.Init(Conf.ItemStackConfig);
        }
        private void Update()
        {
            frame++;
            if (frame % updateDelay != 0) return;
            frame = 0;

            while (MainThreadQueue.Count > 0)
            {
                MainThreadQueue[0].Run();
                MainThreadQueue.RemoveAt(0);
            }
        }
        private void OnPreDisconnectSave(CSteamID steamID, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(steamID);
            if (player == null) 
            {
                return;
            }

            // disable plugin crafting
            player.Player.quests.sendSetFlag(Conf.PluginCraftingFlag, 0);

            if (player.Dead)
            {
                player.Player.life.sendRevive();
                PlayerSpawnpoint spawn = LevelPlayers.getSpawn(isAlt: false);
                if (spawn != null)
                {
                    byte b = MeasurementTool.angleToByte(spawn.angle);
                    player.Player.life.ReceiveRevive(spawn.point + new Vector3(0f, 0.5f, 0f), b);
                }
                else
                {
                    Logger.LogError($"Could not revive {steamID}!");
                }
            }
            else
            {
                TeleportControler.OnPreDisconnectSave(player);
            }
        }
        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            ScavRunControler.OnPlayerDisconnected(player);
            OpenableItemsControler.OnPlayerDisconnected(player);
            QuestExtensionControler.OnPlayerDisconected(player);
            HideoutControler.OnPlayerDisconnect(player);
        }
        private void OnPlayerConnected(UnturnedPlayer player)
        {
            OnPlayerConnectedInner(player);
        }
        private void OnPlayerConnectedInner(UnturnedPlayer player, bool restore = false)
        {
            // enable plugin crafting
            player.Player.quests.sendSetFlag(Conf.PluginCraftingFlag, 1);

            HideoutControler.OnPlayerConnected(player);
            ScavRunControler.OnPlayerConnected(player);

            if (!ScavRunControler.isScavRunActive(player))
            {
                SecureCaseControler.OnPlayerConnected(player);
            }
            if (!PlayerSavedata.fileExists(player.SteamPlayer().playerID, "/Player/Player.dat"))
            {
                setupNewPlayer(player);
            }
            else if(!restore)
            {
                TeleportControler.OnPlayerConnected(player);
            }
        }
        private void OnGetInput(InputInfo inputInfo, ERaycastInfoUsage usage, ref bool shouldAllow)
        {
            switch (usage)
            {
                case ERaycastInfoUsage.Fuel:
                case ERaycastInfoUsage.Refill:
                    HideoutControler.OnGetInput(inputInfo, usage, ref shouldAllow);
                    break;
            }
        }
        private void OnBarricadeStorageRequest(InteractableStorage storage, ServerInvocationContext context, bool quickGrab, ref bool shouldAllow)
        {
            HideoutControler.OnBarricadeStorageRequest(storage, context, quickGrab, ref shouldAllow);
        }
        private void OnEquipmentChanged(PlayerEquipment equipment)
        {
            OpenableItemsControler.OnEquipmentChanged(equipment);

        }
        private void OnFlagChanged(PlayerQuests quests, ushort flagId, short flagValue, ref bool shouldAllow)
        {
            PlayerQuestFlag flag = new PlayerQuestFlag(flagId, flagValue);
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(quests.player);
            quests.setFlag(flagId, flagValue);

            ScavRunControler.OnFlagChanged(player, flag);
            TeleportControler.OnFlagChanged(player, flag);
            SecureCaseControler.OnFlagChanged(player, flag);
            quests.getFlag(flagId, out short newValue);
            shouldAllow = flagValue == newValue; 
        }
        private void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!shouldAllow) return;

            PlacementRestrictionControler.OnBarricadeDeploy(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
            HideoutControler.OnBarricadeDeploy(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
        }
        private void OnBarricadeSalvageRequest(BarricadeDrop barricade, SteamPlayer instigatorClient, ref bool shouldAllow)
        {
            HideoutControler.OnBarricadeSalvageRequest(barricade, instigatorClient, ref shouldAllow);
        }
        private void OnBarricadeSpawned(BarricadeRegion region, BarricadeDrop drop)
        {
            PlacementRestrictionControler.OnBarricadeSpawned(region, drop);
            HideoutControler.OnBarricadeSpawned(region, drop);
        }
        private void OnBarricadeDestroy(BarricadeDrop barricade, byte x, byte y, ushort plant)
        {
            HideoutControler.OnBarricadeDestroy(barricade, x, y, plant);
            PlacementRestrictionControler.OnBarricadeDestroy(barricade, x, y, plant);
        }
        private void OnStructureSpawned(StructureRegion region, StructureDrop drop)
        {
            //TODO: add hideout stuff
        }
        private void OnInteractableConditionCheck(ObjectAsset objectAsset, Player player, ref bool shouldAllow)
        {
            OpenableItemsControler.OnInteractableConditionCheck(objectAsset, player, ref shouldAllow);
        }
        private void OnPrePlayerDead(PlayerLife playerLife)
        {
            DeathAdditionsControler.OnPrePlayerDead(playerLife);
            SecureCaseControler.OnPrePlayerDead(playerLife);
        }
        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            DeathAdditionsControler.OnPlayerDeath(player, cause, limb, murderer);
            QuestExtensionControler.OnPlayerDeath(player, cause, limb, murderer);
        }
        private void OnPlayerRevived(PlayerLife playerLife)
        {
            SecureCaseControler.OnPlayerRevived(playerLife);
            DeathAdditionsControler.OnPlayerRevived(playerLife);
            ScavRunControler.OnPlayerRevived(playerLife);
            TeleportControler.OnPlayerRevived(playerLife);
        }
        private void OnInspect(PlayerEquipment equipment)
        {
            OpenableItemsControler.OnInspect(equipment);
            //TODO: add info for stims
        }
        private void OnThrowableSpawned(UseableThrowable useable, GameObject throwable)
        {
            AirdropControler.OnThrowableSpawned(useable, throwable);
        }
        private void OnPreForceGiveItem(Player player, ushort id, byte amount, ref bool success, ref bool shouldAllow)
        {
            ItemStackController.OnPreForceGiveItem(player, id, amount, ref success, ref shouldAllow);
        }
        private void OnItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            ItemStackController.OnItemSwapped(inventory, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
            if (!shouldAllow) return;
            SecureCaseControler.OnItemSwapped(inventory, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
            if (!shouldAllow) return;
            OpenableItemsControler.OnItemSwapped(inventory, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
        }
        private void OnItemDragged(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            ItemStackController.OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
            if (!shouldAllow) return;
            SecureCaseControler.OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
            if (!shouldAllow) return;
            OpenableItemsControler.OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
            if (!shouldAllow) return;
            UnloadMagControler.OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
        }
        private void OnPreItemAdded(PlayerInventory inventory, Items page, Item item, ref bool didAdditem, ref bool shouldAllow)
        {
            OpenableItemsControler.OnPreItemAdded(page, item, ref didAdditem, ref shouldAllow);
            SecureCaseControler.OnAddItem(inventory, page, item, ref shouldAllow);
        }
        private void OnTakeItem(Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            SecureCaseControler.OnTakeItem(player, x, y, instanceID, to_x, to_y, to_rot, to_page, itemData, ref shouldAllow);
            OpenableItemsControler.OnTakeItem(player, x, y, instanceID, to_x, to_y, to_rot, to_page, itemData, ref shouldAllow);
        }
        private void OnTryAddItem(PlayerInventory inventory, Item item, ref bool autoEquipWeapon, ref bool autoEquipUseable, ref bool autoEquipClothing)
        {
            WeaponModdingControler.PreventAutoEquipOfCraftedGuns(inventory, item, ref autoEquipClothing, ref autoEquipUseable, ref autoEquipClothing);
        }
        private void OnEquipmentStateUpdate(PlayerEquipment equipment)
        {
            UnloadMagControler.ReplaceEmptyMagInGun(equipment);
        }
        private void OnInventoryUpdated(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar itemJ)
        {
            if (ReplaceBypass.Contains(player.CSteamID))
            {
                ReplaceBypass.Remove(player.CSteamID);
                return;
            }

            WeaponModdingControler.HandleAttachmentsOfCraftedGuns(player, inventoryGroup, inventoryIndex, itemJ);

            UnloadMagControler.EmptyEmptyMagVariants(player, inventoryGroup, itemJ);
            UnloadMagControler.ReplaceEmptyMagWithEmptyVarient(player, inventoryGroup, inventoryIndex, itemJ);
            
        }
        private void OnCraft(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow)
        {
            bool replaced = false;
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);
            Blueprint blueprint = ((ItemAsset)Assets.find(EAssetType.ITEM, itemID)).blueprints[blueprintIndex];

            WeaponModdingControler.SaveAttachmentsOfCraftedGun(blueprint, crafting);

            UnloadMagControler.ReplaceEmptyMagazineBlueprintWithFullVariant(blueprint, crafting, ref itemID, ref blueprintIndex, ref shouldAllow, ref replaced);
            ItemStackController.OnCraft(player, blueprint, out bool shouldAddBypass, ref shouldAllow);
            if (shouldAddBypass && !ReplaceBypass.Contains(player.CSteamID))
            {
                ReplaceBypass.Add(player.CSteamID);
                return;
            }

        }
        private void OnZombieDamage(ref DamageZombieParameters parameters, ref bool canDamage)
        {
            UnturnedPlayer player = null;
            if (parameters.instigator is CSteamID)
                player = UnturnedPlayer.FromCSteamID((CSteamID)parameters.instigator);
            else if (parameters.instigator is Player)
                player = UnturnedPlayer.FromPlayer((Player)parameters.instigator);
            
            QuestExtensionControler.OnZombieDamage(player, ref parameters, ref canDamage);
        }
        private void OnZombieDeath(Zombie zombie)
        {
            QuestExtensionControler.OnZombieDeath(zombie);
        }
        private void OnAnimalDamage(ref DamageAnimalParameters parameters, ref bool canDamage)
        {
            UnturnedPlayer player = null;
            if (parameters.instigator is CSteamID)
                player = UnturnedPlayer.FromCSteamID((CSteamID)parameters.instigator);
            else if (parameters.instigator is Player)
                player = UnturnedPlayer.FromPlayer((Player)parameters.instigator);

            QuestExtensionControler.OnAnimalDamage(player, ref parameters, ref canDamage);
        }
        private void OnAnimalDeath(Animal animal)
        {
            QuestExtensionControler.OnAnimalDeath(animal);
        }
        private void OnResourceDamage(CSteamID instigatorSteamID, Transform resource, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
            QuestExtensionControler.OnResourceDamage(resource, pendingTotalDamage, player);
        }
        private void OnStatIncremented(Player player, EPlayerStat stat)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            switch (stat)
            {
                case EPlayerStat.FOUND_FISHES:
                    QuestExtensionControler.OnFishCaught(uPlayer);
                    break;
            }

        }
        #region HelperFunctions

        internal static Dictionary<ushort, T> createDictionaryFromItemExtensions<T>(List<T> itemExtensions) where T : ItemExtension
        {
            Dictionary<ushort, T> itemExtensionsDict = new Dictionary<ushort, T>();
            if (itemExtensions == null)
                return itemExtensionsDict;

            foreach (T itemExtension in itemExtensions)
            {
                if (itemExtension.Id == 0)
                    continue;

                if (itemExtensionsDict.ContainsKey(itemExtension.Id))
                {
                    Logger.LogWarning("Item with Id:" + itemExtension.Id + " is a duplicate!");
                    continue;
                }

                itemExtensionsDict.Add(itemExtension.Id, itemExtension);
            }
            return itemExtensionsDict;
        }
        internal static void setupNewPlayer(UnturnedPlayer player)
        {
            if (Conf?.NewPlayerKitConfig == null)
                return;

            player.Player.life.serverModifyHealth(Conf.NewPlayerKitConfig.Health);
            player.Player.life.serverModifyFood(Conf.NewPlayerKitConfig.Food);
            player.Player.life.serverModifyWater(Conf.NewPlayerKitConfig.Water);
            player.Player.life.serverModifyVirus(Conf.NewPlayerKitConfig.Virus);

            foreach (var item in Conf.NewPlayerKitConfig.KitItems)
            {
                player.GiveItem(item.Id, item.Amount);
            }
        }
        
        private void printPluginInfo()
        {

            Logger.Log("Unturnov II ServerPlugin by SpeedMann Loaded, ");

        }
        #endregion
    }
}
