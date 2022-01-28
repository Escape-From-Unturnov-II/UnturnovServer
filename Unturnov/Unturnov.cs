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
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov
{
    public class Unturnov : RocketPlugin<UnturnovConfiguration>
    {
        public static Unturnov Inst;
        public static UnturnovConfiguration Conf;

        private Dictionary<ushort, CombineDescription> AutoCombineDict;
        private Dictionary<ushort, ushort> MagazineTypes;
        private List<CSteamID> ReplaceBypass;

        #region Load
        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;

            ReplaceBypass = new List<CSteamID>();

            MagazineTypes = createDictionaryFromMagazineExtensions(Conf.UnloadMagBlueprints);
            AutoCombineDict = createDictionaryFromAutoCombine(Conf.AutoCombine);

            printPluginInfo();

            Conf.updateConfig();

            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested += OnCraft;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            UseableConsumeable.onConsumePerformed += OnConsumed;
        }
        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnInventoryUpdated;
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

            #region Mag load/unload logic
            MagazineExtension magazineExtension = Conf.UnloadMagBlueprints.Find(x => x.EmptyMagazineId == P.item.id);
            if (magazineExtension != null)
            {
                if (Conf.Debug)
                {
                    Logger.Log("empty mag");
                }
               
                player.Inventory.sendUpdateAmount(((byte)inventoryGroup), P.x, P.y, (byte)0);
                return;
            }
            ushort emptyMagId;
            if ((MagazineTypes.TryGetValue(P.item.id, out emptyMagId) && P.item.amount <= 0))
            {
                if (Conf.Debug)
                {
                    Logger.Log("replace mag");
                }
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

            #region Check Disable Autocombine
            if (blueprint.outputs.Length > 0)
            {
                foreach(BlueprintOutput outP in  blueprint.outputs)
                {
                    if (AutoCombineDict.ContainsKey(outP.id) && (!ReplaceBypass.Contains(player.CSteamID)))
                    {
                        ReplaceBypass.Add(player.CSteamID);
                        return;
                    }
                }
            }
            #endregion

            #region Load Mag
            MagazineExtension magExt = Conf.UnloadMagBlueprints.Find(x => x.EmptyMagazineId == itemId);
            if (magExt != null)
            {
                // load mag
                PlayerInventory inventory = crafting.player.inventory;
                MagazineExtension.MagazineType magtype = null;

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

                        magtype = magExt.MagazineTypeIds.Find(x => ((ItemAsset)Assets.find(EAssetType.ITEM, x.MagazineId)).blueprints[x.refillAmmoIndex].supplies[0].id == blueprint.supplies[0].id);
                        Item replacement = new Item(magtype.MagazineId, (byte)0, supplyList[0].jar.item.quality);
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
                if (magtype != null)
                {
                    itemID = magtype.MagazineId;
                    blueprintIndex = magtype.refillAmmoIndex;
                }
            }
            #endregion
        }

        private void OnConsumed(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            if (Conf.MultiUseItems.Contains(consumeableAsset.id))
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

        private Dictionary<ushort, ushort> createDictionaryFromMagazineExtensions(List<MagazineExtension> magExtensions)
        {
            Dictionary<ushort, ushort> itemExtensionsDict = new Dictionary<ushort, ushort>();
            if (magExtensions != null)
            {
                foreach (MagazineExtension extension in magExtensions)
                {
                    if (extension.MagazineTypeIds != null)
                    {
                        foreach (MagazineExtension.MagazineType magType in extension.MagazineTypeIds)
                        {
                            if (itemExtensionsDict.ContainsKey(magType.MagazineId))
                            {
                                Logger.LogWarning("Item with Id:" + magType + " is a duplicate!");
                            }
                            else
                            {

                                itemExtensionsDict.Add(magType.MagazineId, extension.EmptyMagazineId);
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
        private void printPluginInfo()
        {

            Logger.Log("Unturnov II ServerPlugin by SpeedMann Loaded, ");

        }
        #endregion
    }
}
