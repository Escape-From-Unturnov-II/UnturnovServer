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

namespace SpeedMann.Unturnov
{
    public class Unturnov : RocketPlugin<UnturnovConfiguration>
    {
        public static Unturnov Inst;
        public static UnturnovConfiguration Conf;
        public static DatabaseManager Database;
        public static bool ModsLoaded = false;
        

        private Dictionary<ushort, CombineDescription> AutoCombineDict;
        private Dictionary<ushort, ushort> MagazineDict;
        private Dictionary<ushort, ItemExtension> GunModdingDict;
        private Dictionary<ushort, ReloadInner> ReloadExtensionByGun;
        private Dictionary<CSteamID, InternalMagReloadState> ReloadExtensionStates;
        private Dictionary<CSteamID, GunAttachments> ModdedGunAttachments;
        
        private List<CSteamID> ReplaceBypass;
        public static List<MainQueueEntry> MainThreadQueue = new List<MainQueueEntry>();

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
            SecureCaseControler.Init(Conf.SecureCaseConfig);
            PlacementRestrictionControler.Init(Conf.PlacementRestrictionConfig);
            HideoutControler.Init(Conf.HideoutConfig);
            OpenableItemsControler.Init();
            QuestExtensionControler.Init();
            DeathAdditionsControler.Init(Conf.DeathDropConfig);

            ReplaceBypass = new List<CSteamID>();
            ReloadExtensionStates = new Dictionary<CSteamID, InternalMagReloadState>();
            ModdedGunAttachments = new Dictionary<CSteamID, GunAttachments>();

            MagazineDict = createDictionaryFromMagazineExtensions(Conf.UnloadMagBlueprints);
            AutoCombineDict = createDictionaryFromAutoCombine(Conf.AutoCombine);
            GunModdingDict = createDictionaryFromItemExtensions(Conf.GunModdingResults);
            ReloadExtensionByGun = createDictionaryFromReloadExtensionsByGun(Conf.ReloadExtensions);

            Conf.updateConfig();

            if (ModsLoaded)
            {
                //TODO: claim hideouts on reload
                Conf.addNames();
            }

            UnturnedPatches.OnPreTryAddItemAuto += OnTryAddItem;

            PlayerQuests.onAnyFlagChanged += OnFlagChanged;

            UnturnedPatches.OnPreAttachMagazine += OnPreAttachMag;
            UnturnedPatches.OnPostAttachMagazine += OnPostAttachMag;
            UseableGun.onChangeMagazineRequested += OnChangeMagazine;
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

            PlayerEquipment.OnInspectingUseable_Global += OnInspect;

            UnturnedPatches.OnPreDisconnectSave += OnPreDisconnectSave;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            U.Events.OnPlayerConnected += OnPlayerConnected;

            Level.onPreLevelLoaded += OnPreLevelLoaded;

            printPluginInfo();
        }

        protected override void Unload()
        {
            UnturnedPatches.Cleanup();
            ScavRunControler.Cleanup();
            QuestExtensionControler.Cleanup();

            Provider.modeConfigData.Gameplay.Timer_Home = oldBedTimer;

            UnturnedPatches.OnPreTryAddItemAuto -= OnTryAddItem;

            PlayerQuests.onAnyFlagChanged -= OnFlagChanged;

            UnturnedPatches.OnPreAttachMagazine -= OnPreAttachMag;
            UnturnedPatches.OnPostAttachMagazine -= OnPostAttachMag;
            UseableGun.onChangeMagazineRequested -= OnChangeMagazine;
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

            UnturnedPatches.OnPreDisconnectSave -= OnPreDisconnectSave;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            U.Events.OnPlayerConnected -= OnPlayerConnected;

            Level.onPreLevelLoaded -= OnPreLevelLoaded;
        }
        #endregion
        private void OnPreLevelLoaded(int level)
        {
            Conf.addNames();
            ModsLoaded = true;
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
        }
        private void OnPlayerConnected(UnturnedPlayer player)
        {
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
            HideoutControler.OnBarricadeSpawned(region, drop);
        }
        private void OnBarricadeDestroy(BarricadeDrop barricade, byte x, byte y, ushort plant)
        {
            HideoutControler.OnBarricadeDestroy(barricade, x, y, plant);
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
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(inventory.player);
            if (GunModdingDict.ContainsKey(item.id) && ModdedGunAttachments.ContainsKey(player.CSteamID))
            {
                // prevent autoequip of crafted guns
                autoEquipClothing = autoEquipUseable = autoEquipWeapon = false;
            }
        }
        private void OnPreAttachMag(UseableGun gun, byte page, byte x, byte y, byte[] hash)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(gun.player);
            if (ReloadExtensionStates.ContainsKey(player.CSteamID)){
                ReloadExtensionStates.Remove(player.CSteamID);
            }

            InternalMagReloadState state = new InternalMagReloadState { newMag = new ItemJarWrapper { page = page }, };
            ReloadExtensionStates.Add(player.CSteamID, state);

            ItemJar gunItemJar = InventoryHelper.getItemJarOfEquiped(player.Player.equipment);

            //save and remove mag
            Item mag = InventoryHelper.getMagFromGun(gunItemJar.item);
            if (mag != null && page != 255)
            {
                state.oldMag = mag;
                InventoryHelper.removeMagFromGun(player.Player.equipment);
                Logger.Log("Removed mag from gun");
            }
        }
        private void OnChangeMagazine(PlayerEquipment equipment, UseableGun gun, Item oldItem, ItemJar newItem, ref bool shouldAllow)
        {
            Logger.Log($"Changed magazine for: {equipment.itemID} old Mag: {(oldItem != null ? oldItem.id.ToString() : "none")} new Mag: {(newItem?.item != null ? newItem.item.id.ToString() : "none")}");

            #region ReloadExtension

            if (newItem?.item != null && ReloadExtensionByGun.TryGetValue(gun.equippedGunAsset.id, out ReloadInner reloadInfo) && reloadInfo.AmmoStackId == newItem.item.id)
            {
                UnturnedPlayer player = UnturnedPlayer.FromPlayer(equipment.player);
                if (!ReloadExtensionStates.TryGetValue(player.CSteamID, out InternalMagReloadState reloadState) && reloadState.newMag == null)
                {
                    Logger.LogError("Error getting saved reload state");
                    return;
                }

                
                // save ammo stack
                Item AmmoStack = new Item(newItem.item.id, newItem.item.amount, newItem.item.quality);
                reloadState.newMag.itemJar = new ItemJar(newItem.x, newItem.y, newItem.rot, AmmoStack);
                reloadState.reloaded = true;
                

                Logger.Log("reloaded with reloadExtension!");
            }

            #endregion

        }
        private void OnPostAttachMag(UseableGun gun)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(gun.player);
            if (ReloadExtensionByGun.TryGetValue(gun.equippedGunAsset.id, out ReloadInner reloadInfo) && ReloadExtensionStates.TryGetValue(player.CSteamID, out InternalMagReloadState reloadState))
            {
                if (reloadState?.newMag?.itemJar?.item?.amount > 0 && reloadState.reloaded)
                {
                    ItemJar newMag = reloadState?.newMag.itemJar;
                    byte newMagAmount;
                    
                    // add remaining ammo from old mag
                    if (newMag.item.amount < reloadInfo.MagazineSize && reloadState.oldMag?.amount > 0 && reloadState.oldMag.id == newMag.item.id)
                    {
                        newMag.item.amount += reloadState.oldMag.amount;
                        reloadState.oldMag.amount = 0;
                        Logger.Log("added old ammo to new mag");
                    }
                    // change ammo to max mag size
                    newMagAmount = newMag.item.amount < reloadInfo.MagazineSize ? newMag.item.amount : reloadInfo.MagazineSize;
                    
                    player.Player.equipment.state[10] = newMagAmount;
                    player.Player.equipment.sendUpdateState();
                    // give remaining ammo
                    Item remaining = new Item(newMag.item.id, (byte)(newMag.item.amount - newMagAmount), newMag.item.quality);
                    safeAddItem(player, remaining, newMag.x, newMag.y, reloadState.newMag.page, newMag.rot);
                    Logger.Log("added remaining ammo to inventory");
                }
                // give old mag
                if (reloadState.oldMag != null)
                {
                    ItemMagazineAsset magAsset = Assets.find(EAssetType.ITEM, reloadState.oldMag.id) as ItemMagazineAsset;
                    if(reloadState.reloaded) 
                    {
                        if(reloadState.oldMag.amount > 0 || (!gun.equippedGunAsset.shouldDeleteEmptyMagazines && !magAsset.deleteEmpty)){
                            player.Inventory.forceAddItem(reloadState.oldMag, false);
                            Logger.Log("added old mag to inventory");
                        }
                    }
                    else
                    {
                        InventoryHelper.setMagForGun(player.Player.equipment, reloadState.oldMag);
                        Logger.Log("restored old mag");
                    }
                }
            }
        }
        private void OnInventoryUpdated(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            if (ReplaceBypass.Contains(player.CSteamID))
            {
                ReplaceBypass.Remove(player.CSteamID);
                return;
            }

            #region Weapon Modding
            if (GunModdingDict.ContainsKey(P.item.id) && ModdedGunAttachments.TryGetValue(player.CSteamID, out GunAttachments attachments))
            {
                ModdedGunAttachments.Remove(player.CSteamID);
                ItemAsset asset = Assets.find(EAssetType.ITEM, P.item.id) as ItemAsset;
                if (asset != null)
                {

                    // get initial state and remove mag and ammo
                    ItemGunAsset gunAsset = (ItemGunAsset)asset;
                    byte[] newState = gunAsset.getState();
                    newState[8] = 0;
                    newState[9] = 0;
                    newState[10] = 0;

                    // check attachments
                    foreach (ushort caliber in gunAsset.attachmentCalibers)
                    {
                        foreach (GunAttachment att in attachments.attachments)
                        {
                            if (!att.set && att.calibers != null && att.calibers.Contains(caliber))
                            {
                                att.SetAttachment(ref newState);
                            }
                        }
                    }

                    // check mag
                    foreach (ushort caliber in gunAsset.magazineCalibers)
                    {
                        if (attachments.magAttachment.calibers != null && attachments.magAttachment.calibers.Contains(caliber))
                        {
                            attachments.magAttachment.SetAttachment(ref newState);
                            break;
                        }
                    }
                    // give incompatible attachments
                    foreach (GunAttachment att in attachments.attachments)
                    {
                        if (!att.set && att.id != 0)
                        {
                            Item item = new Item(att.id, true);
                            if (Conf.Debug)
                            {
                                Logger.Log($"gave incompatible attachment: {item.id}");
                            }
                            if (!player.GiveItem(item))
                            {
                                player.Inventory.forceAddItem(item, false);
                            }
                        }
                    }
                    // give incompatible mag
                    if (!attachments.magAttachment.set && attachments.magAttachment.id != 0)
                    {
                        Item item = new Item(attachments.magAttachment.id, attachments.ammo, 100);
                        if (Conf.Debug)
                        {
                            Logger.Log($"gave incompatible magazine: {item.id}");
                        }
                        if (!player.GiveItem(item))
                        {
                            player.Inventory.forceAddItem(item, false);
                        }
                    }
                    player.Inventory.sendUpdateInvState((byte)inventoryGroup, P.x, P.y, newState);
                }
            }
            #endregion

            #region auto combine
            //TODO: add implementation to combine 2x2 + 1 = 5
            CombineDescription combine;
            if (AutoCombineDict.TryGetValue(P.item.id, out combine))
            {
                List<InventorySearch> foundItems = player.Inventory.search(P.item.id, true, true);
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
                    Logger.Log($"combined {results}x {P.item.id} to {combine.Result.Id}");
                }
                return;
            }
            #endregion

            #region Empty Mag logic
            EmptyMagazineExtension magazineExtension = Conf.UnloadMagBlueprints.Find(x => x.Id == P.item.id);
            if (magazineExtension != null)
            {
                player.Inventory.sendUpdateAmount(((byte)inventoryGroup), P.x, P.y, 0);
                return;
            }

            // change emptied mags to empty variant
            if ((MagazineDict.TryGetValue(P.item.id, out ushort emptyMagId) && P.item.amount <= 0))
            {
                player.Inventory.removeItem((byte)inventoryGroup, inventoryIndex);
                Item replacement = new Item(emptyMagId, (byte)0, P.item.quality);

                safeAddItem(player, replacement, P.x, P.y, (byte)inventoryGroup, P.rot);
                return;
            }
            #endregion
        }
        private void OnCraft(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow)
        {

            ushort itemId = itemID;
            Blueprint blueprint = ((ItemAsset)Assets.find(EAssetType.ITEM, itemID)).blueprints[blueprintIndex];
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);
            PlayerInventory inventory = crafting.player.inventory;

            #region Weapon Modding
            GunAttachments attachments = null;
            // find original Gun and its Attachments
            foreach (BlueprintSupply supply in blueprint.supplies)
            {
                if (GunModdingDict.ContainsKey(supply.id))
                {
                    List<InventorySearch> itemList = inventory.search(supply.id, true, true);
                    if (itemList.Count > 0)
                    {
                        ItemAsset asset = Assets.find(EAssetType.ITEM, itemList[0].jar.item.id) as ItemAsset;
                        if (asset != null)
                        {
                            attachments = new GunAttachments(itemList[0].jar.item.metadata);
                            if (Conf.Debug)
                            {
                                Logger.Log($"Modded weapon with: sight {attachments.attachments[0].id}, tactical {attachments.attachments[1].id}, grip {attachments.attachments[2].id}, barrel {attachments.attachments[3].id}, mag {attachments.magAttachment.id}, ammo {attachments.ammo}");
                            }
                            byte index = player.Inventory.findIndex(itemList[0].page, itemList[0].jar.x, itemList[0].jar.y, out byte found_x,out byte found_y);
                            player.Inventory.updateState(itemList[0].page, index, new byte[18]);
                            break;
                        }
                    }
                }
            }
            // save attachments
            if (attachments != null)
            {
                if (!ModdedGunAttachments.ContainsKey(player.CSteamID))
                {
                    ModdedGunAttachments.Add(player.CSteamID, attachments);
                }
                else
                {
                    ModdedGunAttachments[player.CSteamID] = attachments;
                }
            }
            #endregion

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

            #region Load Mag
            EmptyMagazineExtension magExt = Conf.UnloadMagBlueprints.Find(x => x.Id == itemId);
            if (magExt != null)
            {
                // load mag
                EmptyMagazineExtension.LoadedMagazineVariant loadedMag = null;

                foreach (BlueprintSupply supply in blueprint.supplies)
                {
                    ushort ammount = 0;
                    List<InventorySearch> supplyList = inventory.search(supply.id, false, true);
                    foreach (InventorySearch search in supplyList)
                    {
                        ammount += search.jar.item.amount;
                    }
                    if (ammount <= 0)
                    {
                        break;
                    }
                    List<InventorySearch> itemList = inventory.search(itemID, true, true);
                    if (itemList.Count > 0)
                    {
                        byte index = inventory.getIndex(itemList[0].page, itemList[0].jar.x, itemList[0].jar.y);
                        loadedMag = magExt.LoadedMagazines.Find(x => ((ItemAsset)Assets.find(EAssetType.ITEM, x.Id)).blueprints[x.RefillAmmoBlueprintIndex].supplies[0].id == blueprint.supplies[0].id);
                        if (loadedMag == null)
                        {
                            Logger.LogError($"Error in EmptyMagazineExtension while trying to find replacement blueprint for item: {itemID} and blueprint: {blueprintIndex}");
                            break;
                        }

                        inventory.removeItem(itemList[0].page, index);

                        Item replacement = new Item(loadedMag.Id, (byte)0, supplyList[0].jar.item.quality);
                        if (!ReplaceBypass.Contains(player.CSteamID))
                        {
                            ReplaceBypass.Add(player.CSteamID);
                        }
                        if (!inventory.tryAddItem(replacement, supplyList[0].jar.x, supplyList[0].jar.y, supplyList[0].page, supplyList[0].jar.rot))
                        {
                            inventory.forceAddItem(replacement, false);
                        }
                    }
                }
                if (loadedMag != null)
                {
                    itemID = loadedMag.Id;
                    blueprintIndex = loadedMag.RefillAmmoBlueprintIndex;
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
        private Dictionary<ushort, ushort> createDictionaryFromMagazineExtensions(List<EmptyMagazineExtension> magExtensions)
        {
            Dictionary<ushort, ushort> itemExtensionsDict = new Dictionary<ushort, ushort>();
            if (magExtensions != null)
            {
                foreach (EmptyMagazineExtension extension in magExtensions)
                {
                    if (extension.LoadedMagazines != null)
                    {
                        foreach (EmptyMagazineExtension.LoadedMagazineVariant magType in extension.LoadedMagazines)
                        {
                            if (itemExtensionsDict.ContainsKey(magType.Id))
                            {
                                Logger.LogWarning("Item with Id:" + magType + " is a duplicate!");
                            }
                            else
                            {

                                itemExtensionsDict.Add(magType.Id, extension.Id);
                            }
                        }
                    }
                }
            }
            return itemExtensionsDict;
        }
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
        private Dictionary<ushort, ReloadInner> createDictionaryFromReloadExtensionsByGun(List<ReloadExtension> reloadExtensions)
        {
            Dictionary<ushort, ReloadInner> ReloadExtensionDict = new Dictionary<ushort, ReloadInner>();
            if (reloadExtensions != null)
            {
                foreach (ReloadExtension reloadExtension in reloadExtensions)
                {
                    foreach (ReloadInner reloadInner in reloadExtension.Compatibles)
                    {
                        reloadInner.AmmoStackId = reloadExtension.AmmoStack.Id;

                        foreach (ItemExtension itemExtension in reloadInner.Gun)
                        {
                            if (ReloadExtensionDict.ContainsKey(itemExtension.Id))
                            {
                                Logger.LogWarning("ReloadExtension Gun with Id:" + itemExtension.Id + " is defined twice!");
                            }
                            else
                            {
                                ReloadExtensionDict.Add(itemExtension.Id, reloadInner);
                            }
                        }
                    }
                }
            }
            return ReloadExtensionDict;
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
            player.Player.life.ReceiveHealth(Conf.NewPlayerKitConfig.Health);
            player.Player.life.ReceiveFood(Conf.NewPlayerKitConfig.Food);
            player.Player.life.ReceiveWater(Conf.NewPlayerKitConfig.Water);
            player.Player.life.ReceiveVirus(Conf.NewPlayerKitConfig.Virus);

            foreach (var item in Conf.NewPlayerKitConfig.KitItems)
            {
                player.GiveItem(item.Id, item.Amount);
            }
        }
        internal static void safeAddItem(UnturnedPlayer player, Item item, byte x, byte y, byte page, byte rot)
        {
            if (!player.Inventory.tryAddItem(item, x, y, page, rot))
            {
                if (!player.GiveItem(item))
                {
                    player.Inventory.forceAddItem(item, false);
                }
            }
        }
        internal static void stackOrAddItem()
        {
            //TODO: implemet
        }
        private void printPluginInfo()
        {

            Logger.Log("Unturnov II ServerPlugin by SpeedMann Loaded, ");

        }
        #endregion
    }
}
