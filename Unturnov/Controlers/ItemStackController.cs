using Google.Protobuf.WellKnownTypes;
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
        private static Dictionary<ushort, byte> ItemStackSizes = new Dictionary<ushort, byte>();
        private static Dictionary<ushort, CombineDescription> AutoCombineDict;

        internal static void Init(List<ItemExtensionAmount> stackableItems, List<CombineDescription> autoCombine)
        {
            ItemStackSizes = CreateDictionaryFromItemExtensions(stackableItems);
            AutoCombineDict = CreateDictionaryFromAutoCombine(autoCombine);
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
                if (AutoCombineDict.ContainsKey(outP.id))
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
                HandleStackableItemRewards(player, id, amount, stackSize);
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
                || itemJar_1 == null
                || itemJar_0.item.id != itemJar_1.item.id
                || !ItemStackSizes.TryGetValue(itemJar_0.item.id, out byte stackSize))
            {
                return;
            }
            shouldAllow = false;
            InventoryHelper.stackItem(inventory, itemJar_0, page_0, itemJar_1, page_1, index_1, stackSize);
        }

        public static void OnItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
        }
        public static void OnInventoryUpdated(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar itemJ)
        {
            if (!AutoCombineDict.TryGetValue(itemJ.item.id, out CombineDescription combine))
            {
                return;
            }
            int foundAmmount = InventoryHelper.searchAmount(player.Inventory, out List<InventorySearch> foundItems, itemJ.item.id);

            if (foundAmmount < combine.RequiredAmount)
            {
                return;
            }
            int results = foundAmmount / combine.RequiredAmount;
            int ammmountToRemove = results * combine.RequiredAmount;

            foreach (InventorySearch item in foundItems)
            {
                ammmountToRemove -= item.jar.item.amount;
                if (ammmountToRemove >= 0)
                {
                    byte index = player.Inventory.getIndex(item.page, item.jar.x, item.jar.y);
                    player.Inventory.removeItem(item.page, index);
                }
                else
                {
                    ammmountToRemove *= -1;
                    player.Inventory.sendUpdateAmount(item.page, item.jar.x, item.jar.y, (byte)ammmountToRemove);
                    break;
                }

            }
            for (byte i = 0; i < results; i++)
            {
                player.Inventory.forceAddItem(new Item(combine.Result.Id, 1, 100), false);
            }
            Logger.Log($"combined {results}x {itemJ.item.id} to {combine.Result.Id}");
        }
        private static Dictionary<ushort, CombineDescription> CreateDictionaryFromAutoCombine(List<CombineDescription> autoCombine)
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
                        continue;
                    }
                    autoCombineDict.Add(craftDesc.Id, craftDesc);
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

        private static void HandleStackableItemRewards(Player player, ushort id, byte amount, byte stackSize)
        {
            InventoryHelper.FillExisting(player, id, stackSize, ref amount);
            while (amount > 0)
            {
                byte itemAmount = stackSize;
                if (amount < stackSize)
                {
                    itemAmount = amount;
                }
                amount -= itemAmount;
                Item item = new Item(id, itemAmount, 100);
                player.inventory.forceAddItem(item, auto: true);
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
