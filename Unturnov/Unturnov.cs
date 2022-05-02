using HarmonyLib;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SpeedMann.Unturnov.Models.GunAttachments;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov
{
    public class Unturnov : RocketPlugin<UnturnovConfiguration>
    {
        public static Unturnov Inst;
        public static UnturnovConfiguration Conf;
        public static bool ModsLoaded = false;

        private Dictionary<ushort, CombineDescription> AutoCombineDict;
        private Dictionary<ushort, ushort> MagazineDict;
        private Dictionary<ushort, ItemExtension> MultiUseDict;
        private Dictionary<ushort, ItemExtension> GunModdingDict;
        private Dictionary<ushort, ReloadInner> ReloadExtensionByGun;
        private Dictionary<CSteamID, ItemJarWrapper> ReloadExtensionStates;
        private Dictionary<CSteamID, GunAttachments> ModdedGunAttachments;
        
        private List<CSteamID> ReplaceBypass;
        public static List<MainQueueEntry> MainThreadQueue = new List<MainQueueEntry>();

        private int updateDelay = 30;
        private int frame = 0;

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

            UnturnedPrivateFields.Init();
            UnturnedPatches.Init();
            ScavRunControler.Init();

            ReplaceBypass = new List<CSteamID>();
            ReloadExtensionStates = new Dictionary<CSteamID, ItemJarWrapper>();
            ModdedGunAttachments = new Dictionary<CSteamID, GunAttachments>();

            MagazineDict = createDictionaryFromMagazineExtensions(Conf.UnloadMagBlueprints);
            AutoCombineDict = createDictionaryFromAutoCombine(Conf.AutoCombine);
            MultiUseDict = createDictionaryFromItemExtensions(Conf.MultiUseItems);
            GunModdingDict = createDictionaryFromItemExtensions(Conf.GunModdingResults);
            ReloadExtensionByGun = createDictionaryFromReloadExtensionsByGun(Conf.ReloadExtensions);

            

            Conf.updateConfig();

            if (ModsLoaded)
            {
                Conf.addNames();
            }

            UnturnedPatches.OnPreTryAddItemAuto += OnTryAddItem;

            PlayerQuests.onAnyFlagChanged += OnFlagChanged;

            UnturnedPatches.OnPreAttachMagazine += OnPreAttachMag;
            UnturnedPatches.OnPostAttachMagazine += OnPostAttachMag;
            UseableGun.onChangeMagazineRequested += OnChangeMagazine;
            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested += OnCraft;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            UseableConsumeable.onConsumePerformed += OnConsumed;
            UseableConsumeable.onPerformingAid += OnAid;
            BarricadeManager.onDeployBarricadeRequested += OnBarricadeDeploy;

            UnturnedPatches.OnPrePlayerDead += OnPlayerDead;
            UnturnedPatches.OnPostPlayerRevive += OnPlayerRevived;

            UnturnedPatches.OnPrePlayerDraggedItem += OnItemDragged;
            UnturnedPatches.OnPrePlayerSwappedItem += OnItemSwapped;
            UnturnedPatches.OnPrePlayerAddItem -= OnAddItem;
            ItemManager.onTakeItemRequested += OnTakeItem;

            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            U.Events.OnPlayerConnected += OnPlayerConnected;

            Level.onPreLevelLoaded += OnPreLevelLoaded;

            printPluginInfo();
        }
        protected override void Unload()
        {
            UnturnedPatches.Cleanup();
            ScavRunControler.Cleanup();

            UnturnedPatches.OnPreTryAddItemAuto -= OnTryAddItem;

            PlayerQuests.onAnyFlagChanged -= OnFlagChanged;

            UnturnedPatches.OnPreAttachMagazine -= OnPreAttachMag;
            UnturnedPatches.OnPostAttachMagazine -= OnPostAttachMag;
            UseableGun.onChangeMagazineRequested -= OnChangeMagazine;
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested -= OnCraft;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            UseableConsumeable.onConsumePerformed -= OnConsumed;
            UseableConsumeable.onPerformingAid -= OnAid;

            UnturnedPatches.OnPrePlayerDead -= OnPlayerDead;
            UnturnedPatches.OnPostPlayerRevive -= OnPlayerRevived;

            UnturnedPatches.OnPrePlayerDraggedItem -= OnItemDragged;
            UnturnedPatches.OnPrePlayerSwappedItem -= OnItemSwapped;
            UnturnedPatches.OnPrePlayerAddItem -= OnAddItem;
            ItemManager.onTakeItemRequested -= OnTakeItem;

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
        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            ScavRunControler.OnPlayerDisconnected(player);
        }
        private void OnPlayerConnected(UnturnedPlayer player)
        {
            if (!ScavRunControler.isScavRunActive(player))
            {
                ScavRunControler.OnPlayerConnected(player);
                SecureCaseControler.OnPlayerConnected(player);
            }
        }
        private void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            ScavRunControler.OnFlagChanged(quests, flag);
            TeleportControler.OnFlagChanged(quests, flag);
        }
        private void onBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            // check if ItemBarricaeAsset is in PlacementRestrictions
            // check hit Transform if this is the object the barricade got placed on
            // if not try find object below new barricade
            /*
            Regions.tryGetCoordinate(new Vector3(point.x, point.y, point.z), out byte x, out byte y);
            List<RegionCoordinate> coordinates = new List<RegionCoordinate>() { new RegionCoordinate(x, y) };
            List<Transform> transforms = new List<Transform>();
            // find object bellow
            // replace with object search
            StructureManager.getStructuresInRadius(new Vector3(point.x, point.y - 0.5f, point.z), 2, coordinates, transforms);

            foreach (var transform in transforms)
            {
                // check if valid object for barricade
            }
            */
        }
        private void OnPlayerDead(PlayerLife playerLife)
        {
            SecureCaseControler.OnPlayerDead(playerLife);
        }
        private void OnPlayerRevived(PlayerLife playerLife)
        {
            SecureCaseControler.OnPlayerRevived(playerLife);
        }
        private void OnItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            SecureCaseControler.OnItemSwapped(inventory, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
        }
        private void OnItemDragged(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            SecureCaseControler.OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
        }
        private void OnAddItem(PlayerInventory inventory, Items page, Item item, ref bool shouldAllow)
        {
            SecureCaseControler.OnAddItem(inventory, page, item, ref shouldAllow);
        }
        private void OnTakeItem(Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            SecureCaseControler.OnTakeItem(player, x, y, instanceID, to_x, to_y, to_rot, to_page, itemData, ref shouldAllow);
        }
        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if(cause != EDeathCause.SUICIDE){

                if(Conf.DeathDrops?.Count > 0)
                {
                    // defaults to index 0 if no flag it set or found
                    Item item = new Item(Conf.DeathDrops[0].Id, true);
                    if(Conf.DeathDropFlag != 0 && player.Player.quests.getFlag(Conf.DeathDropFlag, out short dropFlagValue))
                    {
                        DeathDrop drop = Conf.DeathDrops.Find(x => x.RequiredFalgValue == dropFlagValue);
                        if (drop != null)
                        {
                            item = new Item(drop.Id, true);
                        }
                    }
                    if (Conf.Debug)
                    {
                        Logger.Log($"deathdrop {item.id} dropped");
                    }
                    ItemManager.dropItem(item, player.Position, true, false, true);
                }
                UnturnedPlayer murderPlayer = UnturnedPlayer.FromCSteamID(murderer);
                // TODO: implement quest extension falg id check
            }
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
            ReloadExtensionStates.Add(player.CSteamID, new ItemJarWrapper{ page = page });
        }
        private void OnChangeMagazine(PlayerEquipment equipment, UseableGun gun, Item oldItem, ItemJar newItem, ref bool shouldAllow)
        {
            Logger.Log($"Changed magazine for: {equipment.itemID} old Mag: {(oldItem != null ? oldItem.id.ToString() : "none")} new Mag: {(newItem?.item != null ? newItem.item.id.ToString() : "none")}");

            #region ReloadExtension

            if (newItem?.item != null && ReloadExtensionByGun.TryGetValue(gun.equippedGunAsset.id, out ReloadInner reloadInfo) && reloadInfo.AmmoStackId == newItem.item.id)
            {
                UnturnedPlayer player = UnturnedPlayer.FromPlayer(equipment.player);
                if (!ReloadExtensionStates.TryGetValue(player.CSteamID, out ItemJarWrapper reloadState))
                {
                    Logger.LogError("Error getting saved reload state");
                    return;
                }


                // save ammo stack
                Item AmmoStack = new Item(newItem.item.id,newItem.item.amount, newItem.item.quality);
                reloadState.itemJar = new ItemJar(newItem.x, newItem.y, newItem.rot, AmmoStack);

                Logger.Log("reloaded with reloadExtension!");
            }

            #endregion

        }
        private void OnPostAttachMag(UseableGun gun)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(gun.player);
            if (ReloadExtensionByGun.TryGetValue(gun.equippedGunAsset.id, out ReloadInner reloadInfo) && ReloadExtensionStates.TryGetValue(player.CSteamID, out ItemJarWrapper reloadState) && reloadState?.itemJar?.item?.amount > 0)
            {
                // change ammo to max mag size
                byte newMagAmount = reloadState.itemJar.item.amount < reloadInfo.MagazineSize ? reloadState.itemJar.item.amount : reloadInfo.MagazineSize;               
                player.Player.equipment.state[10] = newMagAmount;

                // give remaining ammo
                Item remaining = new Item(reloadState.itemJar.item.id, (byte)(reloadState.itemJar.item.amount - newMagAmount), reloadState.itemJar.item.quality);
                safeAddItem(player, remaining, reloadState.itemJar.x, reloadState.itemJar.y, reloadState.page, reloadState.itemJar.rot);
            }
        }
      
        private void OnInventoryUpdated(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            if (ReplaceBypass.Contains(player.CSteamID))
            {
                if (Conf.Debug)
                {
                    Logger.Log("bypass replace");
                }
                ReplaceBypass.Remove(player.CSteamID);
                return;
            }

            #region Weapon Modding
            if (GunModdingDict.ContainsKey(P.item.id) && ModdedGunAttachments.TryGetValue(player.CSteamID, out GunAttachments attachments))
            {
                ModdedGunAttachments.Remove(player.CSteamID);
                Asset asset = Assets.find(EAssetType.ITEM, P.item.id);
                if (asset != null && asset is ItemGunAsset)
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
                        Asset asset = Assets.find(EAssetType.ITEM, itemList[0].jar.item.id);
                        if (asset != null && asset is ItemGunAsset)
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

        private void OnAid(Player instigator, Player target, ItemConsumeableAsset asset, ref bool shouldAllow)
        {
            UseConsumeable(instigator, asset);
        }
        private void OnConsumed(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            UseConsumeable(instigatingPlayer, consumeableAsset);
        }


        #region HelperFunctions
        private void UseConsumeable(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            if (MultiUseDict.ContainsKey(consumeableAsset.id))
            {
                byte page = instigatingPlayer.equipment.equippedPage;
                byte x = instigatingPlayer.equipment.equipped_x;
                byte y = instigatingPlayer.equipment.equipped_y;
                byte index = instigatingPlayer.inventory.getIndex(page, x, y);
                ItemJar itemJar = instigatingPlayer.inventory.getItem(page, index);

                if (itemJar.item.amount > 1)
                {
                    instigatingPlayer.inventory.sendUpdateAmount(page, x, y, (byte)(itemJar.item.amount - 1));
                }
                else
                {
                    instigatingPlayer.inventory.removeItem(page, index);
                }
            }
        }
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
        internal static bool tryGetFoundationSet(string name, out List<ItemExtension> whitelist)
        {
            whitelist = new List<ItemExtension>();
            foreach (FoundationSet list in Conf.FoundationSets)
            {
                if (list.Name.Equals(name))
                {
                    whitelist = list.WhitelistedItems;
                    return true;
                }
            }

            return false;
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
