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

        private Dictionary<ushort, CombineDescription> AutoCombineDict;
        private Dictionary<ushort, ushort> MagazineDict;
        private Dictionary<ushort, ItemExtension> MultiUseDict;
        private Dictionary<ushort, ItemExtension> GunModdingDict;
        private Dictionary<CSteamID, GunAttachments> ModdedGunAttachments;
        private List<CSteamID> ReplaceBypass;

        #region Load
        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;

            UnturnedPrivateFields.Init();
            UnturnedPatches.Init();
            MessageHandler.Init();
            OpenableItemsHandler.Init();

            ReplaceBypass = new List<CSteamID>();
            ModdedGunAttachments = new Dictionary<CSteamID, GunAttachments>();

            MagazineDict = createDictionaryFromMagazineExtensions(Conf.UnloadMagBlueprints);
            AutoCombineDict = createDictionaryFromAutoCombine(Conf.AutoCombine);
            MultiUseDict = createDictionaryFromItemExtensions(Conf.MultiUseItems);
            GunModdingDict = createDictionaryFromItemExtensions(Conf.GunModdingResults);
            

            printPluginInfo();

            Conf.updateConfig();

            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnInventoryUpdated;
            PlayerEquipment.OnInspectingUseable_Global += OnInspect;
            PlayerCrafting.onCraftBlueprintRequested += OnCraft;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            UseableConsumeable.onConsumePerformed += OnConsumed;
        }
        protected override void Unload()
        {
            UnturnedPatches.Cleanup();

            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnInventoryUpdated;
            PlayerEquipment.OnInspectingUseable_Global -= OnInspect;
            PlayerCrafting.onCraftBlueprintRequested -= OnCraft;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            UseableConsumeable.onConsumePerformed -= OnConsumed;
        }
        #endregion

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if(cause != EDeathCause.SUICIDE){

                if(Conf.DeathDrops?.Count > 0)
                {
                    Item item = new Item(Conf.DeathDrops[0].ItemId, true);
                    if(Conf.DeathDropFlag != 0)
                    {
                        short dropFlagValue;
                        if (player.Player.quests.getFlag(Conf.DeathDropFlag, out dropFlagValue))
                        {
                            DeathDrop drop = Conf.DeathDrops.Find(x => x.RequiredFalgValue == dropFlagValue);
                            if(drop != null)
                            {
                                item = new Item(drop.ItemId, true);
                            }
                        }
                    }
                    if (Conf.Debug)
                    {
                        Logger.Log($"deathdrop {item.id} dropped");
                    }
                    ItemManager.dropItem(item, player.Position, true, false, true);
                }
            }
        }

        private void OnInspect(PlayerEquipment equipment)
        {
            equipment.state = OpenableItemsHandler.checkState(equipment.asset, equipment.state);
            equipment.sendUpdateState();

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
                    newState[0] = 0;
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
                        if (!att.set)
                        {
                            Item item = new Item(att.id, true);
                            if (!player.GiveItem(item))
                            {
                                player.Inventory.forceAddItem(item, false);
                            }
                        }
                    }
                    // give incompatible mag
                    if (!attachments.magAttachment.set)
                    {
                        Item item = new Item(attachments.magAttachment.id, attachments.ammo, 100);
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
                        player.Inventory.forceAddItem(new Item(combine.ResultId, true), false);
                    }
                    Logger.Log($"combined {results}x {P.item.id} to {combine.ResultId}");
                }
                return;
            }
            #endregion

            #region Mag change/unload logic
            EmptyMagazineExtension magazineExtension = Conf.UnloadMagBlueprints.Find(x => x.ItemId == P.item.id);
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

                if (!player.Inventory.tryAddItem(replacement, P.x, P.y, (byte)inventoryGroup, P.rot))
                {
                    if (!player.GiveItem(replacement))
                    {
                        player.Inventory.forceAddItem(replacement, false);
                    }
                }
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
            EmptyMagazineExtension magExt = Conf.UnloadMagBlueprints.Find(x => x.ItemId == itemId);
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
                        inventory.removeItem(itemList[0].page, index);

                        loadedMag = magExt.LoadedMagazines.Find(x => ((ItemAsset)Assets.find(EAssetType.ITEM, x.ItemId)).blueprints[x.RefillAmmoBlueprintIndex].supplies[0].id == blueprint.supplies[0].id);
                        Item replacement = new Item(loadedMag.ItemId, (byte)0, supplyList[0].jar.item.quality);
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
                    itemID = loadedMag.ItemId;
                    blueprintIndex = loadedMag.RefillAmmoBlueprintIndex;
                }
            }
            #endregion
        }

        private void OnConsumed(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
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
                            if (itemExtensionsDict.ContainsKey(magType.ItemId))
                            {
                                Logger.LogWarning("Item with Id:" + magType + " is a duplicate!");
                            }
                            else
                            {

                                itemExtensionsDict.Add(magType.ItemId, extension.ItemId);
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
                    if (craftDesc.ItemId == 0)
                    {
                        Logger.LogWarning("Resource Item with invalid Id");
                        continue;
                    }

                    if (autoCombineDict.ContainsKey(craftDesc.ItemId))
                    {
                        Logger.LogWarning("Resource Item with Id:" + craftDesc.ItemId + " is a duplicate!");
                    }
                    else
                    {
                        autoCombineDict.Add(craftDesc.ItemId, craftDesc);
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
                    if (itemExtension.ItemId == 0)
                        continue;

                    if (itemExtensionsDict.ContainsKey(itemExtension.ItemId))
                    {
                        Logger.LogWarning("Item with Id:" + itemExtension.ItemId + " is a duplicate!");
                    }
                    else
                    {
                        itemExtensionsDict.Add(itemExtension.ItemId, itemExtension);
                    }

                }
            }
            return itemExtensionsDict;
        }
        private void printPluginInfo()
        {

            Logger.Log("Unturnov II ServerPlugin by SpeedMann Loaded, ");

        }
        #endregion
    }
}
