using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Controlers
{
    internal class UnloadMagControler
    {
        private static Dictionary<ushort, ushort> FullToEmptyMagazineDict;
        private static Dictionary<ushort, EmptyMagazineExtension> EmptyMageDict;
        internal static void Init(List<EmptyMagazineExtension> UnloadMagExtensions)
        {
            FullToEmptyMagazineDict = createDictionaryFromMagazineExtensions(UnloadMagExtensions);
            EmptyMageDict = Unturnov.createDictionaryFromItemExtensions(UnloadMagExtensions);
        }
        internal static void ReplaceEmptymagazineBlueprintWithFullVariant(Blueprint blueprint, PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow, ref bool replaced)
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
        internal static void EmptyEmptyMagVariants(UnturnedPlayer player, InventoryGroup inventoryGroup, ItemJar itemJ)
        {
            if (!EmptyMageDict.ContainsKey(itemJ.item.id))
                return;

            player.Inventory.sendUpdateAmount((byte)inventoryGroup, itemJ.x, itemJ.y, 0);
        }
        internal static void ReplaceEmptyMagWithEmptyVarient(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar itemJ)
        {
            if (itemJ.item.amount > 0 || !FullToEmptyMagazineDict.TryGetValue(itemJ.item.id, out ushort emptyMagId))
                return;

            player.Inventory.removeItem((byte)inventoryGroup, inventoryIndex);
            Item replacement = new Item(emptyMagId, (byte)0, itemJ.item.quality);

            InventoryHelper.addItem(player, replacement, itemJ.x, itemJ.y, (byte)inventoryGroup, itemJ.rot);
            Logger.Log($"Replaced mag {itemJ.item.id} with empty variant {emptyMagId}");
        }

        #region HelperFunctions
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

                InventoryHelper.addItem(player, replacement, magazineList[0].jar.x, magazineList[0].jar.y, magazineList[0].page, magazineList[0].jar.rot);
                
                
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
