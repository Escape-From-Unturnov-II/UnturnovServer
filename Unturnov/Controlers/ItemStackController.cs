using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI.Relational;
using Rocket.Core.Assets;
using Rocket.Core.Logging;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Controlers
{
    internal class ItemStackController
    {
        private static ItemStackConfig Conf;
        private static Dictionary<ushort, byte> ItemStackSizes = new Dictionary<ushort, byte>();
        private static Dictionary<ushort, ReplaceFullDescription> AutoReplaceDict;

        internal static void Init(ItemStackConfig config)
        {
            Conf = config;
            ItemStackSizes = CreateDictionaryFromItemExtensions(config.StackableItems);
            AutoReplaceDict = CreateDictionaryFromAutoCombine(config.ReplaceFull);
        }
        public static void OnCraft(UnturnedPlayer player, Blueprint blueprint, out bool shouldAddBypass, ref bool shouldAllow)
        {
            shouldAddBypass = false;
            Logger.Log($"Crafted {blueprint.id} supplies {blueprint.supplies.Length} outputs {blueprint.outputs.Length}");
            if (blueprint.supplies.Length == 1
                && blueprint.outputs.Length == 1
                && blueprint.outputs[0].id == blueprint.supplies[0].id
                && blueprint.outputs[0].amount == 2)
            {
                HandleStackSplitting(player.Inventory, blueprint.supplies[0].id);
                shouldAllow = false;
                shouldAddBypass = true;
                return;
            }

            foreach (BlueprintOutput outP in blueprint.outputs)
            {
                if (AutoReplaceDict.ContainsKey(outP.id))
                {
                    shouldAddBypass = true;
                    return;
                }

            }
        }
        public static void OnPreForceGiveItem(Player player, ushort id, byte amount, ref bool success, ref bool shouldAllow)
        {
            if (!shouldAllow)
            {
                return;
            }
            shouldAllow = false;
            ItemAsset itemAsset = Assets.find(EAssetType.ITEM, id) as ItemAsset;
            if (itemAsset == null || itemAsset.isPro)
            {
                success = false;
                return;
            }
            if (ItemStackSizes.TryGetValue(id, out byte stackSize))
            {
                HandleStackableItemRewards(player.inventory, id, amount, stackSize);
                success = true;
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                Item item = new Item(id, EItemOrigin.ADMIN);
                player.inventory.forceAddItem(item, auto: true);
            }
            success = true;
            return;
        }
        public static void OnItemDragged(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            byte index_0 = inventory.items[page_0].getIndex(x_0, y_0);
            byte index_1 = inventory.items[page_1].getIndex(x_1, y_1);
            ItemJar itemJar_0 = inventory.items[page_0].getItem(index_0);
            ItemJar itemJar_1 = inventory.items[page_1].getItem(index_1);
            if (itemJar_0 == null
                || itemJar_1 == null)
            {
                return;
            }
            InventoryItemWrapper item_0 = new InventoryItemWrapper(itemJar_0, page_0, index_0);
            InventoryItemWrapper item_1 = new InventoryItemWrapper(itemJar_1, page_1, index_1);

            if (ItemStackSizes.Count > 0 
                && TryStackItem(inventory, item_0, item_1))
            {
                shouldAllow = false;
                return;
            }

            if (item_0.asset == null
                || item_1.asset == null)
            {
                return;
            }

            if (Conf.AutoAddMagazines
                && TryAddMagazine(inventory, item_0, item_1))
            {
                shouldAllow = false;
                return;
            }

        }

        public static void OnItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
        }
        public static void CombineItems(PlayerInventory inventory, ushort itemId, int requiredAmount)
        {
            if (!AutoReplaceDict.TryGetValue(itemId, out ReplaceFullDescription replace))
            {
                return;
            }
            int foundAmount = InventoryHelper.searchAmount(inventory, out List<InventorySearch> foundItems, itemId);

            if (foundAmount < requiredAmount)
            {
                return;
            }
            int results = foundAmount / replace.RequiredAmount;
            int ammmountToRemove = results * replace.RequiredAmount;

            foreach (InventorySearch item in foundItems)
            {
                ammmountToRemove -= item.jar.item.amount;
                if (ammmountToRemove >= 0)
                {
                    byte index = inventory.getIndex(item.page, item.jar.x, item.jar.y);
                    inventory.removeItem(item.page, index);
                }
                else
                {
                    ammmountToRemove *= -1;
                    inventory.sendUpdateAmount(item.page, item.jar.x, item.jar.y, (byte)ammmountToRemove);
                    break;
                }

            }
            for (byte i = 0; i < results; i++)
            {
                inventory.forceAddItem(new Item(replace.Result.Id, 1, 100), false);
            }
        }
        private static void ReplaceItem(PlayerInventory inventory, InventorySearch itemToReplace)
        {
            if (!AutoReplaceDict.TryGetValue(itemToReplace.jar.item.id, out ReplaceFullDescription replace))
            {
                return;
            }
            HandleStackableItemRewards(inventory, itemToReplace.jar.item.id, 1, replace.RequiredAmount);
        }
        private static bool TryStackItem(PlayerInventory inventory, InventoryItemWrapper item_0, InventoryItemWrapper item_1)
        {
            if (item_0.itemJar.item.id != item_1.itemJar.item.id
                || !ItemStackSizes.TryGetValue(item_0.itemJar.item.id, out byte stackSize))
            {
                return false;
            }
            InventoryHelper.stackItem(inventory, item_0.itemJar, item_0.page, item_1.itemJar, item_1.page, item_1.index, stackSize);
            return true;
        }
        private static bool TryAddMagazine(PlayerInventory inventory, InventoryItemWrapper item_0, InventoryItemWrapper item_1)
        {
            if(item_0.asset.type != EItemType.GUN 
                || item_1.asset.type != EItemType.MAGAZINE)
            {
                return false;
            }
            ItemGunAsset gunAsset = item_0.asset as ItemGunAsset;
            ItemMagazineAsset magAsset = item_1.asset as ItemMagazineAsset;
            if (!InventoryHelper.IsCompatible(gunAsset, magAsset))
            {
                return false;
            }

            Item oldMagazine = InventoryHelper.getMagFromGun(item_0.itemJar.item);
            InventoryHelper.setMagForGun(item_0.itemJar.item, item_1.itemJar.item);
            inventory.removeItem(item_1.page, item_1.index);
            if (oldMagazine != null)
            {
                InventoryHelper.forceAddItem(inventory, oldMagazine, item_1.page, item_1.itemJar.x, item_1.itemJar.y, item_1.itemJar.rot);
            }
            return true;
        }
        private static Dictionary<ushort, ReplaceFullDescription> CreateDictionaryFromAutoCombine(List<ReplaceFullDescription> autoCombine)
        {
            Dictionary<ushort, ReplaceFullDescription> autoCombineDict = new Dictionary<ushort, ReplaceFullDescription>();
            if (autoCombine != null)
            {
                foreach (ReplaceFullDescription replaceDesc in autoCombine)
                {
                    if (replaceDesc.Id == 0)
                    {
                        Logger.LogWarning("Resource Item with invalid Id");
                        continue;
                    }

                    if (autoCombineDict.ContainsKey(replaceDesc.Id))
                    {
                        Logger.LogWarning("Resource Item with Id:" + replaceDesc.Id + " is a duplicate!");
                        continue;
                    }
                    if (replaceDesc.RequiredAmount < 1)
                    {
                        ItemAsset itemAsset = Assets.find(EAssetType.ITEM, replaceDesc.Id) as ItemAsset;
                        if (itemAsset == null)
                        {
                            Logger.LogWarning($"Could not get item amount of ReplaceDescription with Id: {replaceDesc.Id}, it was skipped!");
                            continue;
                        }
                        replaceDesc.RequiredAmount = itemAsset.amount;
                    }
                    autoCombineDict.Add(replaceDesc.Id, replaceDesc);
                }
            }
            return autoCombineDict;
        }
        private static Dictionary<ushort, byte> CreateDictionaryFromItemExtensions<T>(List<T> itemExtensionAmount) where T : ItemExtensionAmount
        {
            Dictionary<ushort, byte> itemExtensionsDict = new Dictionary<ushort, byte>();
            if (itemExtensionAmount == null)
            {
                return itemExtensionsDict;
            }

            foreach (T itemExtension in itemExtensionAmount)
            {
                if (itemExtension.Id == 0)
                    continue;

                if (itemExtensionsDict.ContainsKey(itemExtension.Id))
                {
                    Logger.LogWarning("Item with Id:" + itemExtension.Id + " is a duplicate!");
                    continue;
                }
                byte amount = itemExtension.Amount;

                if (itemExtension.Amount < 1)
                {
                    ItemAsset itemAsset = Assets.find(EAssetType.ITEM, itemExtension.Id) as ItemAsset;
                    if (itemAsset == null)
                    {
                        Logger.LogWarning($"Could not get item amount of StackableItem with Id: {itemExtension.Id}, it was skipped!");
                        continue;
                    }
                    amount = itemAsset.amount;
                }
                itemExtensionsDict.Add(itemExtension.Id, amount);

            }
            return itemExtensionsDict;
        }

        private static void HandleStackableItemRewards(PlayerInventory inventory, ushort id, byte amount, byte stackSize)
        {
            HandleStackableItemRewardsInner(inventory, id, amount, stackSize, out bool filledStack);

            while (filledStack && AutoReplaceDict.TryGetValue(id, out ReplaceFullDescription replace))
            {
                //TODO: find all full stack and call handleStackable again
                //amount = InventoryHelper.searchAmount(inventory, out List<InventorySearch> foundItems, id);
                //InventoryHelper.FillExisting(foundItems)
            } 

            
        }
        private static void HandleStackableItemRewardsInner(PlayerInventory inventory, ushort id, byte amount, byte stackSize, out bool filledStack)
        {
            InventoryHelper.FillExisting(inventory, id, amount, ref amount, out filledStack);
            while (amount > 0)
            {
                byte itemAmount = stackSize;
                if (amount < stackSize)
                {
                    itemAmount = amount;
                }
                amount -= itemAmount;
                Item item = new Item(id, itemAmount, 100);
                inventory.forceAddItem(item, auto: true);
            }
        }
        private static void HandleStackSplitting(PlayerInventory inventory, ushort itemId)
        {
            if (!InventoryHelper.tryFindSplittable(inventory, out InventorySearch foundItem, itemId))
            {
                return;
            }
            Logger.Log($"Stack splitting supplies {itemId}x{foundItem.jar.item.amount}");

            byte newAmount = (byte)decimal.Floor(foundItem.jar.item.amount / 2);
            inventory.sendUpdateAmount(foundItem.page, foundItem.jar.x, foundItem.jar.y, (byte)(foundItem.jar.item.amount - newAmount));
            
            Item item = new Item(itemId, newAmount, 100);
            inventory.forceAddItem(item, auto: true);
            Logger.Log($"Split {itemId} to {foundItem.jar.item.amount}&{newAmount}");
        }

    }
}
