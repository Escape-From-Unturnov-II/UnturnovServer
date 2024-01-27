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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Controlers
{
    internal class ItemStackController
    {
        private static Dictionary<ushort, byte> ItemStackSizes = new Dictionary<ushort, byte>();
        internal static void Init(List<ItemExtensionAmount> stackableItems)
        {
            ItemStackSizes = CreateDictionaryFromItemExtensions(stackableItems);
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

            int newAmmount = itemJar_0.item.amount + itemJar_1.item.amount;

            if (newAmmount > stackSize)
            {
                //TODO: handle stacks > stackSize
                Logger.Log($"Could not set new amount {newAmmount} stack of {itemJar_0.item.id} was full stacksize: {stackSize}");
                return;
            }

            inventory.removeItem(page_1, index_1);
            inventory.sendUpdateAmount(page_0, x_0, y_0, (byte)newAmmount);
            Logger.Log($"Set new amount {newAmmount} stack of {itemJar_0.item.id} with stacksize: {stackSize}");
            shouldAllow = false;
        }
        public static void OnItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            OnItemDragged(inventory, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
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
    }
}
