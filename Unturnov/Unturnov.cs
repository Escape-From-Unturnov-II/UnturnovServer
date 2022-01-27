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

        private List<CSteamID> DisabledAutocraft;
        private Dictionary<ushort, CraftDescription> AutoCraftDict;
        private Dictionary<ushort, ushort> MagazineTypes;
        private List<CSteamID> MagReplaceBypass;

        #region Load
        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;

            UnturnedPrivateFields.Init();
            MagReplaceBypass = new List<CSteamID>();

            MagazineTypes = createDictionaryFromMagazineExtensions(Conf.UnloadMagBlueprints);
            //DisabledAutocraft = new List<CSteamID>();
            //AutoCraftDict = createDictionaryFromAutoCraftlist(Conf.AutoCraft);

            printPluginInfo();

            Conf.updateConfig();

            //U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture += OnGestureChanged;
            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested += OnCraft;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;

        }
        protected override void Unload()
        {
            //U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture -= OnGestureChanged;
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnInventoryUpdated;
            PlayerCrafting.onCraftBlueprintRequested -= OnCraft;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
        }
        #endregion

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            DisabledAutocraft.Remove(player.CSteamID);
        }
        private void OnGestureChanged(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            if (gesture == UnturnedPlayerEvents.PlayerGesture.InventoryOpen)
            {
                if (!DisabledAutocraft.Contains(player.CSteamID))
                    DisabledAutocraft.Add(player.CSteamID);
                if (Conf.Debug)
                {
                    Logger.Log("Disabled AutoCrafting for: " + player.CharacterName);
                }
            }
            else if (gesture == UnturnedPlayerEvents.PlayerGesture.InventoryClose)
            {
                DisabledAutocraft.Remove(player.CSteamID);
                Logger.Log("Reanabled AutoCrafting for: " + player.CharacterName);
            }
        }
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
            if (MagReplaceBypass.Contains(player.CSteamID))
            {
                if (Conf.Debug)
                {
                    Logger.Log("bypass replace");
                }
                MagReplaceBypass.Remove(player.CSteamID);
                return;
            }
            
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
            }
        }
        private void OnCraft(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow)
        {

            ushort itemId = itemID;
            MagazineExtension magExt = Conf.UnloadMagBlueprints.Find(x => x.EmptyMagazineId == itemId);
            if (magExt != null)
            {
                // load mag
                PlayerInventory inventory = crafting.player.inventory;
                UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);
                Blueprint blueprint = ((ItemAsset)Assets.find(EAssetType.ITEM, itemID)).blueprints[blueprintIndex];
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
                        return;
                    }
                    List<InventorySearch> itemList = inventory.search(itemID, true, true);
                    if (itemList.Count > 0)
                    {
                        byte index = inventory.getIndex(itemList[0].page, itemList[0].jar.x, itemList[0].jar.y);
                        inventory.removeItem(itemList[0].page, index);

                        magtype = magExt.MagazineTypeIds.Find(x => ((ItemAsset)Assets.find(EAssetType.ITEM, x.MagazineId)).blueprints[x.refillAmmoIndex].supplies[0].id == blueprint.supplies[0].id);
                        Item replacement = new Item(magtype.MagazineId, (byte)0, supplyList[0].jar.item.quality);
                        if (!MagReplaceBypass.Contains(player.CSteamID))
                        {
                            MagReplaceBypass.Add(player.CSteamID);
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
        }

        #region HelperFunctions
        private static bool findItems(PlayerInventory inventory, ushort itemId, int count, out List<ItemJarWrapper> items)
        {
            items = new List<ItemJarWrapper>();

            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (inventory.items[page] == null) continue;
                byte itemc = inventory.getItemCount(page);
                for (byte index = 0; index < itemc; index++)
                {
                    ItemJar itemJ = inventory.getItem(page, index);
                    if (itemJ.item.id == itemId)
                    {
                        items.Add(new ItemJarWrapper
                        {
                            page = page,
                            index = index,
                            itemJar = itemJ,
                        });
                        if (items.Count == count)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static void sendAutoCraft(UnturnedPlayer player, CraftDescription craftDesc)
        {
            if (Conf.Debug)
            {
                Logger.Log($"AutoCraft ItemId: {craftDesc.BlueprintItemId} BlueprintIndex: {craftDesc.BlueprintIndex}");
            }
            player.Player.crafting.sendCraft(craftDesc.BlueprintItemId, craftDesc.BlueprintIndex, true);
        }

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
        private Dictionary<ushort, CraftDescription> createDictionaryFromAutoCraftlist(List<CraftDescription> autoCraft)
        {
            Dictionary<ushort, CraftDescription> itemExtensionsDict = new Dictionary<ushort, CraftDescription>();
            if (autoCraft != null)
            {
                foreach (CraftDescription craftDesc in autoCraft)
                {
                    if (craftDesc.ResourceItemId == 0)
                    {
                        Logger.LogWarning("Resource Item with invalid Id");
                        continue;
                    }

                    if (itemExtensionsDict.ContainsKey(craftDesc.ResourceItemId))
                    {
                        Logger.LogWarning("Resource Item with Id:" + craftDesc.ResourceItemId + " is a duplicate!");
                    }
                    else
                    {
                        itemExtensionsDict.Add(craftDesc.ResourceItemId, craftDesc);
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
