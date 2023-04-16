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
using SpeedMann.Unturnov.Classes;
using SpeedMann.Unturnov.Controlers;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using static SpeedMann.Unturnov.Models.GunAttachments;
using Logger = Rocket.Core.Logging.Logger;
using SDG.Framework.Devkit;
using static HarmonyLib.Code;
using Rocket.API;

namespace SpeedMann.Unturnov
{
    public class Unturnov : RocketPlugin<UnturnovConfiguration>
    {
        public static Unturnov Inst;
        public static UnturnovConfiguration Conf;
        public static DatabaseManager Database;
        public static bool ModsLoaded = false;
        

        private Dictionary<ushort, CombineDescription> AutoCombineDict;     
        
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
                { "container_item_restricted", "You are not allowed to store this {0} in the secure container!" },
            };

        #region Load
        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            Database = new DatabaseManager();

            // force set bed timer
            oldBedTimer = Provider.modeConfigData.Gameplay.Timer_Home;
            Provider.modeConfigData.Gameplay.Timer_Home = Conf.BedTimer;

            UnturnedPrivateFields.Init();
            UnturnedPatches.Init();
            ScavRunControler.Init();
            TeleportControler.Init(Conf.TeleportConfig);
            SecureCaseControler.Init(Conf.SecureCaseConfig);
            PlacementRestrictionControler.Init(Conf.PlacementRestrictionConfig);
            HideoutControler.Init(Conf.HideoutConfig);
            OpenableItemsControler.Init();
            QuestExtensionControler.Init();
            DeathAdditionsControler.Init(Conf.DeathDropConfig);
            WeaponModdingControler.Init(Conf.GunModdingResults);
            UnloadMagControler.Init(Conf.UnloadMagBlueprints);
            JsonManager.Init(Directory);
            

            ReplaceBypass = new List<CSteamID>();
            
            AutoCombineDict = createDictionaryFromAutoCombine(Conf.AutoCombine);


            Conf.updateConfig();

            UnturnedPatches.OnPreTryAddItemAuto += OnTryAddItem;

            PlayerQuests.onAnyFlagChanged += OnFlagChanged;

            UnturnedPatches.OnPreEquipmentUpdateState += OnEquipmentStateUpdate;
            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested += OnCraft;

            BarricadeManager.onDeployBarricadeRequested += OnBarricadeDeploy;
            BarricadeManager.onBarricadeSpawned += OnBarricadeSpawned;
            StructureManager.onStructureSpawned += OnStructureSpawned;
            UnturnedPatches.OnPreDestroyBarricade += OnBarricadeDestroy;

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
        }

        protected override void Unload()
        {
            UnturnedPatches.Cleanup();
            ScavRunControler.Cleanup();
            TeleportControler.Cleanup();
            QuestExtensionControler.Cleanup();
            HideoutControler.Cleanup();
            PlacementRestrictionControler.Cleanup();
            AirdropControler.Cleaup();

            Provider.modeConfigData.Gameplay.Timer_Home = oldBedTimer;

            UnturnedPatches.OnPreTryAddItemAuto -= OnTryAddItem;

            PlayerQuests.onAnyFlagChanged -= OnFlagChanged;

            UnturnedPatches.OnPreEquipmentUpdateState -= OnEquipmentStateUpdate;
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested -= OnCraft;

            BarricadeManager.onDeployBarricadeRequested -= OnBarricadeDeploy;
            BarricadeManager.onBarricadeSpawned -= OnBarricadeSpawned;
            StructureManager.onStructureSpawned -= OnStructureSpawned;
            UnturnedPatches.OnPreDestroyBarricade -= OnBarricadeDestroy;

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

            Inst = null;
            Conf = null;
        }
        #endregion
        private void OnPreLevelLoaded(int level)
        {
            Conf.addNames();
            AirdropControler.Init(Conf.AirdropSignals);
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

            // disable plugin crafting
            player.Player.quests.sendSetFlag(Conf.PluginCraftingFlag, 0);

            if (player == null) return;

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
        }
        private void OnEquipmentChanged(PlayerEquipment equipment)
        {
            OpenableItemsControler.OnEquipmentChanged(equipment);

        }
        private void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            ScavRunControler.OnFlagChanged(quests, flag);
            TeleportControler.OnFlagChanged(quests, flag);
        }
        private void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!shouldAllow) return;

            PlacementRestrictionControler.OnBarricadeDeploy(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
            HideoutControler.OnBarricadeDeploy(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
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
        }
        private void OnInspect(PlayerEquipment equipment)
        {
            OpenableItemsControler.OnInspect(equipment);
        }
        private void OnThrowableSpawned(UseableThrowable useable, GameObject throwable)
        {
            AirdropControler.OnThrowableSpawned(useable, throwable);
        }
        private void OnItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            SecureCaseControler.OnItemSwapped(inventory, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
            OpenableItemsControler.OnItemSwapped(inventory, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
        }
        private void OnItemDragged(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            SecureCaseControler.OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
            OpenableItemsControler.OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
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
            #region auto combine
            //TODO: add implementation to combine 2x2 + 1 = 5
            CombineDescription combine;
            if (AutoCombineDict.TryGetValue(itemJ.item.id, out combine))
            {
                List<InventorySearch> foundItems = player.Inventory.search(itemJ.item.id, true, true);
                if(foundItems.Count() >= combine.RequiredAmount)
                {
                    int results = foundItems.Count() / combine.RequiredAmount;
                    int turns = combine.RequiredAmount;
                    foreach (InventorySearch item in foundItems)
                    {
                        byte index = player.Inventory.getIndex(item.page, item.jar.x, item.jar.y);
                        player.Inventory.removeItem(item.page, index);
                    }
                    for (byte i = 0; i < results; i++)
                    {
                        player.Inventory.forceAddItem(new Item(combine.Result.Id, true), false);
                    }
                    Logger.Log($"combined {results}x {itemJ.item.id} to {combine.Result.Id}");
                }
                return;
            }
            #endregion

        }
        private void OnCraft(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow)
        {
            bool replaced = false;
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);
            Blueprint blueprint = ((ItemAsset)Assets.find(EAssetType.ITEM, itemID)).blueprints[blueprintIndex];

            WeaponModdingControler.SaveAttachmentsOfCraftedGun(blueprint, crafting);

            UnloadMagControler.ReplaceEmptyMagazineBlueprintWithFullVariant(blueprint, crafting, ref itemID, ref blueprintIndex, ref shouldAllow, ref replaced);
            #region Check Disable Autocombine
            foreach (BlueprintOutput outP in blueprint.outputs)
            {
                if (AutoCombineDict.ContainsKey(outP.id) && (!ReplaceBypass.Contains(player.CSteamID)))
                {
                    ReplaceBypass.Add(player.CSteamID);
                    return;
                }
            }

            #endregion
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
       
        private Dictionary<ushort, CombineDescription> createDictionaryFromAutoCombine(List<CombineDescription> autoCombine)
        {
            Dictionary<ushort, CombineDescription> autoCombineDict = new Dictionary<ushort, CombineDescription>();
            if (autoCombine != null)
            {
                foreach (CombineDescription craftDesc in autoCombine)
                {
                    if (craftDesc.Id == 0)
                    {
                        Logger.LogWarning("Resource Item with invalid Id");
                        continue;
                    }

                    if (autoCombineDict.ContainsKey(craftDesc.Id))
                    {
                        Logger.LogWarning("Resource Item with Id:" + craftDesc.Id + " is a duplicate!");
                    }
                    else
                    {
                        autoCombineDict.Add(craftDesc.Id, craftDesc);
                    }

                }
            }
            return autoCombineDict;
        }
        internal static Dictionary<ushort, T> createDictionaryFromItemExtensions<T>(List<T> itemExtensions) where T : ItemExtension
        {
            Dictionary<ushort, T> itemExtensionsDict = new Dictionary<ushort, T>();
            if (itemExtensions != null)
            {
                foreach (T itemExtension in itemExtensions)
                {
                    if (itemExtension.Id == 0)
                        continue;

                    if (itemExtensionsDict.ContainsKey(itemExtension.Id))
                    {
                        Logger.LogWarning("Item with Id:" + itemExtension.Id + " is a duplicate!");
                    }
                    else
                    {
                        itemExtensionsDict.Add(itemExtension.Id, itemExtension);
                    }

                }
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
