using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;
using Logger = Rocket.Core.Logging.Logger;


/*
Full Mag:
    Blueprint_0_State_Transfer
    Blueprint_0_Type Gear 
    Blueprint_0_Supplies 1
    Blueprint_0_Supply_0_ID [Full mag variant id]
    Blueprint_0_Outputs 2
    Blueprint_0_Output_0_ID [Empty mag variant id]
    Blueprint_0_Output_1_ID [Loaded ammo variant id]
    Blueprint_0_Build 30

    Actions 2
    Action_0_Type Blueprint
    Action_0_Source [Full mag variant id]
    // add all load blueprints to this list
    Action_0_Blueprints 1 
    Action_0_Blueprint_0_Index [Index of the load blueprint]
    Action_0_Key Refill
    Action_1_Type Blueprint
    Action_1_Source [Full mag variant id]
    // add all unload blueprints to this list
    Action_1_Blueprints 1 
    Action_1_Blueprint_0_Index [Index of the unload blueprint]
    Action_1_Text Unload
    Action_1_Tooltip Unload Magazine.

Empty Mag:
    Type Magazine
    Amount [Same as Full variant]
    Calibers 1
    Caliber_0 0 // Caliber 0 should prevent any gun from using the mag

    Blueprint_0_Type Ammo
    Blueprint_0_Supplies 1
    Blueprint_0_Supply_0_ID [ammo variant id]
    Blueprint_0_Build 30
 */
namespace SpeedMann.Unturnov.Controlers
{
    internal class UnloadMagControler
    {
        private static Dictionary<ushort, ushort> FullToEmptyMagazineDict;
        private static Dictionary<ushort, EmptyMagazineExtension> EmptyMageDict;
        internal static void Init(List<EmptyMagazineExtension> unloadMagExtensions)
        {
            FindMatchingBlueprintsForEmptyMags(unloadMagExtensions);
            FullToEmptyMagazineDict = createDictionaryFromMagazineExtensions(unloadMagExtensions);
            EmptyMageDict = Unturnov.createDictionaryFromItemExtensions(unloadMagExtensions);
        }
        public static void OnItemDragged(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            /*
            byte index_0 = inventory.items[page_0].getIndex(x_0, y_0);
            byte index_1 = inventory.items[page_1].getIndex(x_1, y_1);
            ItemJar itemJar_0 = inventory.items[page_0].getItem(index_0);
            ItemJar itemJar_1 = inventory.items[page_1].getItem(index_1);
            if (FullToEmptyMagazineDict.ContainsKey())
            {

            }
            //TODO: add check for set magazines to refill them
            shouldAllow = false;
            InventoryHelper.stackItem(inventory, itemJar_0, page_0, itemJar_1, page_1, index_1, stackSize);
            */
        }
        internal static void ReplaceEmptyMagInGun(PlayerEquipment equipment)
        {
            if (equipment?.state == null || equipment.state.Length < 11)
                return;

            if (equipment.state[10] > 0)
                return;

            Item mag = InventoryHelper.getMagFromGun(equipment);
            if (mag == null || !FullToEmptyMagazineDict.TryGetValue(mag.id, out ushort emptyMagId))
                return;

            byte[] bytes = equipment.state;
            GunAttachments.setMag(ref bytes, emptyMagId, mag.durability,  0);
            equipment.state = bytes;
            equipment.sendUpdateState();
        }
        /*
         * Redirects the Blueprints to the equivalent blueprint of the loaded alternative (matching is done by supply[0] id)
         */
        internal static void ReplaceEmptyMagazineBlueprintWithFullVariant(Blueprint blueprint, PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow, ref bool replaced)
        {
            if (replaced || !shouldAllow || !EmptyMageDict.TryGetValue(itemID, out EmptyMagazineExtension magExtension))
                return;

            UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);

            EmptyMagazineExtension.LoadedMagazineVariant fullMagVariant = tryFindFullMagazineVariant(player, blueprint, magExtension, itemID, blueprintIndex);

            if (fullMagVariant == null)
                return;

            
            itemID = fullMagVariant.Id;
            blueprintIndex = fullMagVariant.RefillAmmoBlueprintIndex;
            replaced = true;
        }
        /*
         * When getting an empty mag the value will be changed to 0
         */
        internal static void EmptyEmptyMagVariants(UnturnedPlayer player, InventoryGroup inventoryGroup, ItemJar itemJ)
        {
            if (!EmptyMageDict.ContainsKey(itemJ.item.id))
                return;

            player.Inventory.sendUpdateAmount((byte)inventoryGroup, itemJ.x, itemJ.y, 0);
        }
        /*
         * All magazines defined in LoadedMagazines get replaced with the empty version when empty
         */
        internal static void ReplaceEmptyMagWithEmptyVarient(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar itemJ)
        {
            //TODO: find a way to replace on amount update on craft
            if (itemJ.item.amount > 0 || !FullToEmptyMagazineDict.TryGetValue(itemJ.item.id, out ushort emptyMagId))
                return;

            player.Inventory.removeItem((byte)inventoryGroup, inventoryIndex);
            Item replacement = new Item(emptyMagId, (byte)0, itemJ.item.quality);

            InventoryHelper.forceAddItem(player.Inventory, replacement, itemJ.x, itemJ.y, (byte)inventoryGroup, itemJ.rot);
            Logger.Log($"Replaced mag {itemJ.item.id} with empty variant {emptyMagId}");
        }

        #region HelperFunctions
        private static void FindMatchingBlueprintsForEmptyMags(List<EmptyMagazineExtension> unloadMagExtensions)
        {
            List<LoadedMagazineWrapper> loadedVariantWrappers = new List<LoadedMagazineWrapper>();
            for (int i = 0; i < unloadMagExtensions.Count; i++)
            {
                loadedVariantWrappers.Clear();
                var emptyMagExtension = unloadMagExtensions[i];
                if (!TryLoadEmptyMagRequirements(emptyMagExtension, loadedVariantWrappers, out ItemAsset emptyMagAsset))
                {
                    continue;
                }

                foreach (var emptyBlueprint in emptyMagAsset.blueprints)
                {
                    if (emptyBlueprint.type != EBlueprintType.AMMO || emptyBlueprint.supplies.Length <= 0)
                    {
                        continue;
                    }
                    
                    if (!TryFindLoadedMagVariant(emptyBlueprint, loadedVariantWrappers))
                    {
                        Logger.LogError($"Could not find any loaded mag variant for {emptyMagExtension.Id} / blueprint {emptyBlueprint.supplies[0].id}!");
                    }
                }
            }
        }
        private static bool TryLoadEmptyMagRequirements(EmptyMagazineExtension emptyMagExtension, List<LoadedMagazineWrapper> listToFill, out ItemAsset emptyMagAsset)
        {
            listToFill.Clear();
            emptyMagAsset = Assets.find(EAssetType.ITEM, emptyMagExtension.Id) as ItemAsset;
            if (emptyMagAsset == null)
            {
                Logger.LogError($"Could not find empty mag {emptyMagExtension.Id}!");
                return false;
            }
            foreach (EmptyMagazineExtension.LoadedMagazineVariant loadedVariant in emptyMagExtension.LoadedMagazines)
            {
                ItemAsset loadedMagItem = Assets.find(EAssetType.ITEM, loadedVariant.Id) as ItemAsset;
                if (emptyMagAsset == null)
                {
                    Logger.LogError($"Could not find loaded mag variant {loadedVariant.Id} for {emptyMagExtension.Id}!");
                    continue;
                }
                listToFill.Add(new LoadedMagazineWrapper(loadedVariant, loadedMagItem));
            }
            if (listToFill.Count <= 0)
            {
                Logger.LogError($"Could not find any loaded mag variants for {emptyMagExtension.Id}!");
                return false;
            }
            return true;
        }
        private static bool TryFindLoadedMagVariant(Blueprint emptyBlueprint, List<LoadedMagazineWrapper> loadedMagVariants)
        {
            foreach (var loadedVariantWrapper in loadedMagVariants)
            {
                for (int y = 0; y < loadedVariantWrapper.itemAsset.blueprints.Count; y++)
                {
                    var loadedBlueprint = loadedVariantWrapper.itemAsset.blueprints[y];
                    if (loadedBlueprint.type != EBlueprintType.AMMO || emptyBlueprint.supplies.Length <= 0)
                    {
                        continue;
                    }
                    if (loadedBlueprint.supplies[0].id == emptyBlueprint.supplies[0].id)
                    {
                        int oldIndex = loadedVariantWrapper.loadedMagazineVariant.RefillAmmoBlueprintIndex;
                        
                        if (oldIndex != y)
                        {
                            loadedVariantWrapper.loadedMagazineVariant.RefillAmmoBlueprintIndex = (byte)y;
                            Logger.LogWarning($"Loaded mag blueprint index {oldIndex} of {loadedVariantWrapper.loadedMagazineVariant.Id} was changed to {y}");
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        private static EmptyMagazineExtension.LoadedMagazineVariant tryFindFullMagazineVariant(UnturnedPlayer player, Blueprint blueprint, EmptyMagazineExtension magExtension, ushort itemId, byte blueprintIndex)
        {
            EmptyMagazineExtension.LoadedMagazineVariant fullVariant = null;

            foreach (BlueprintSupply supply in blueprint.supplies)
            {
                if (InventoryHelper.findAmmo(player.Inventory, supply.id, out List<InventorySearch> foundAmmo) <= 0)
                    break;

                List<InventorySearch> magazineList = player.Inventory.search(itemId, true, true);
                if (magazineList.Count <= 0)
                    break;

                byte index = player.Inventory.getIndex(magazineList[0].page, magazineList[0].jar.x, magazineList[0].jar.y);
                fullVariant = magExtension.LoadedMagazines.Find(x => ((ItemAsset)Assets.find(EAssetType.ITEM, x.Id))?.blueprints[x.RefillAmmoBlueprintIndex].supplies[0].id == blueprint.supplies[0].id);
                if (fullVariant == null)
                {
                    Logger.LogError($"Error in EmptyMagazineExtension while trying to find replacement blueprint for item: {itemId} and blueprint: {blueprintIndex}");
                    break;
                }

                ItemJar removedItem = player.Inventory.getItem(magazineList[0].page, index);
                Logger.Log($"Replacing item at {removedItem.x} {removedItem.y} with {magazineList[0].jar.x} {magazineList[0].jar.y}");
                player.Inventory.removeItem(magazineList[0].page, index);

                Item replacement = new Item(fullVariant.Id, (byte)0, magazineList[0].jar.item.quality);
                if (!Unturnov.ReplaceBypass.Contains(player.CSteamID))
                {
                    Unturnov.ReplaceBypass.Add(player.CSteamID);
                }

                InventoryHelper.forceAddItem(player.Inventory, replacement, magazineList[0].jar.x, magazineList[0].jar.y, magazineList[0].page, magazineList[0].jar.rot);
                
                
            }
            return fullVariant;
        }
        private static Dictionary<ushort, ushort> createDictionaryFromMagazineExtensions(List<EmptyMagazineExtension> magExtensions)
        {
            Dictionary<ushort, ushort> fullToEmptyMagDict = new Dictionary<ushort, ushort>();
            if (magExtensions != null)
            {
                foreach (EmptyMagazineExtension emptyMagVarient in magExtensions)
                {
                    if (emptyMagVarient.LoadedMagazines != null)
                    {
                        foreach (EmptyMagazineExtension.LoadedMagazineVariant fullMagVarient in emptyMagVarient.LoadedMagazines)
                        {
                            if (fullToEmptyMagDict.ContainsKey(fullMagVarient.Id))
                            {
                                Logger.LogWarning("Full mag variant with Id:" + fullMagVarient.Id + " is a duplicate!");
                            }
                            else
                            {
                                fullToEmptyMagDict.Add(fullMagVarient.Id, emptyMagVarient.Id);
                            }
                        }
                    }
                }
            }
            return fullToEmptyMagDict;
        }
        #endregion
    }
}
