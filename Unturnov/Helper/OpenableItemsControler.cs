using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    internal static class OpenableItemsControler
    {
        internal static Dictionary<ushort, OpenableItem> OpenableItemsDict;
        internal static Dictionary<ushort, List<ushort>> WhitelistedItems;
        internal static Dictionary<CSteamID, OpenedItem> OpenItems;
        private static byte Keylength = 4;

        internal static void Init()
        {
            OpenItems = new Dictionary<CSteamID, OpenedItem>();
            OpenableItemsDict = createDictionaryFromItemExtensions(Unturnov.Conf.OpenableItemsConfig.OpenableItems);
            WhitelistedItems = createWhitelistDictionary(Unturnov.Conf.OpenableItemsConfig.OpenableItems);

            foreach (KeyValuePair<ushort, OpenableItem> pair in OpenableItemsDict)
            {
                string tableName = pair.Value.TableName;
                if (tableName != "")
                {
                    Unturnov.Database.CheckItemStorageSchema(tableName);
                }
                else
                {
                    Logger.LogError($"No TableName set for item: {pair.Value.Name}[{pair.Key}]");
                }

            }
        }

        public static void OnInteractableConditionCheck(ObjectAsset objectAsset, Player player, ref bool shouldAllow)
        {
            Logger.Log($"Object interaction for {objectAsset.id} from {player.name}!");

            //shouldAllow = false;
        }
        public static void OnEquipmentChanged(PlayerEquipment equipment)
        {
            CSteamID steamID = UnturnedPlayer.FromPlayer(equipment.player).CSteamID;
            if (OpenItems.TryGetValue(steamID, out OpenedItem openedItem))
            {
                Unturnov.Database.SetItems(openedItem.tableName, ref openedItem.storageId, openedItem.stroage);
                OpenItems.Remove(steamID);
                equipment.player.inventory.closeStorageAndNotifyClient();
            }
        }
        public static void OnInspect(PlayerEquipment equipment)
        {
            CSteamID steamID = UnturnedPlayer.FromPlayer(equipment.player).CSteamID;
            if (OpenItems.TryGetValue(steamID, out OpenedItem openedItem))
            {
                Unturnov.Database.SetItems(openedItem.tableName, ref openedItem.storageId, openedItem.stroage);
                OpenItems.Remove(steamID);
                equipment.player.inventory.closeStorageAndNotifyClient();
            }
            if (TryOpenItem(equipment, out byte[] newState))
            {
                equipment.state = newState;
                equipment.sendUpdateState();
            }
        }
        public static void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (OpenItems.TryGetValue(player.CSteamID, out OpenedItem openedItem))
            {
                Unturnov.Database.SetItems(openedItem.tableName, ref openedItem.storageId, openedItem.stroage);
                OpenItems.Remove(player.CSteamID);
            }

        }

        public static void OnPreItemAdded(Items page, Item item, ref bool didAdditem, ref bool shouldAllow)
        {
            shouldAllow = true;

            if (page.page == (byte)InventoryGroup.Storage)
            {
                object target = page.onStateUpdated.Target;
                if (target is PlayerInventory)
                {
                    UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(((PlayerInventory)target).player);
                    if (isItemStorageOpen(uPlayer) && !isWhitelisted(uPlayer, item.id))
                    {
                        didAdditem = false;
                        shouldAllow = false;
                    }
                }
            }
        }
        public static void OnItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(inventory.player);

            // Hand check
            if (page_0 != (byte)InventoryGroup.Storage && page_1 != (byte)InventoryGroup.Storage
                || page_0 == (byte)InventoryGroup.Storage && page_1 == (byte)InventoryGroup.Storage
                || !isItemStorageOpen(player))
                return;

            if (Unturnov.Conf.Debug)
                Logger.Log("Swapped Item to openedItem");

            byte otherPage = page_0;
            byte added_x = x_0;
            byte added_y = y_0;

            if (page_1 != (byte)InventoryGroup.Storage)
            {
                otherPage = page_1;
                added_x = x_1;
                added_y = y_1;
            }

            var index = inventory.items[otherPage].getIndex(added_x, added_y);
            if (index == byte.MaxValue)
                return;


            var itemJar = inventory.items[otherPage].getItem(index);

            if (!isWhitelisted(player, itemJar.item.id))
            {
                shouldAllow = false;
                notifyNotAllowed(player, itemJar.item.id);
            }
        }
        public static void OnTakeItem(Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            if (isItemStorageOpen(uPlayer) && to_page == (byte)InventoryGroup.Storage && !isWhitelisted(uPlayer, itemData.item.id))
            {
                shouldAllow = false;
                notifyNotAllowed(uPlayer, itemData.item.id);
            }
        }
        public static void OnItemDragged(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(inventory.player);

            if (page_1 != (byte)InventoryGroup.Storage || !isItemStorageOpen(player))
                return;


            if (Unturnov.Conf.Debug)
                Logger.Log("Dragged Item to openedItem");

            var index = inventory.getIndex(page_0, x_0, y_0);
            if (index == byte.MaxValue || page_1 >= PlayerInventory.PAGES - 1 || inventory.items[page_1] == null ||
                inventory.getItemCount(page_1) >= 200)
                return;

            var itemJar = inventory.getItem(page_0, index);
            if (itemJar == null || !inventory.checkSpaceDrag(page_1, x_0, y_0, itemJar.rot, x_1, y_1, rot_1,
                itemJar.size_x, itemJar.size_y, page_0 == page_1))
                return;

            if (!isWhitelisted(player, itemJar.item.id))
            {
                shouldAllow = false;
                notifyNotAllowed(player, itemJar.item.id);
            }
        }

        internal static byte[] checkState(ItemAsset asset, byte[] state, OpenableItem openableItem, Items storage, out uint storageId)
        {
            storageId = 0;

            if (asset == null)
            {
                return new byte[0];
            }
            byte[] defaulState = asset.getState();

            int oldLength = state.Length;
            int newLength = Keylength + defaulState.Length;

            if (state.Length != newLength)
            {
                // increase state array to hold storageId
                byte[] newState = new byte[newLength];

                //TODO: add error handling for item sate resize by other plugins / game update

                Unturnov.Database.SetItems(openableItem.TableName, ref storageId, storage);

                int i = 0;
                while (i < state.Length)
                {
                    if (i < newState.Length)
                    {
                        newState[i] = state[i];
                    }
                    else
                    {
                        Logger.LogError($"Current state of item: {asset.name} was longer than newState");
                        break;
                    }
                    i++;
                }
                byte[] storageIdArray = convertStorageId(storageId);
                for (i = 0; i < storageIdArray.Length && i + defaulState.Length < newState.Length; i++)
                {
                    newState[defaulState.Length + i] = storageIdArray[i];
                }

                state = newState;
            }
            else
            {
                storageId = convertStorageId(asset, state);
            }

            Logger.Log($"Inspected Openable Item {asset.id} with sate Length default: {defaulState.Length} old: {oldLength} new: {state.Length} storageID: {storageId}");
            return state;
        }
        internal static bool TryOpenItem(PlayerEquipment equipment, out byte[] newState)
        {
            newState = new byte[0];
            if (!OpenableItemsDict.TryGetValue(equipment.asset.id, out OpenableItem oItem))
                return false;

            Items storage = new Items(PlayerInventory.STORAGE);

            storage.resize(oItem.Width, oItem.Height);

            newState = checkState(equipment.asset, equipment.state, oItem, storage, out uint storageId);
            if (newState.Length <= 0)
                return false;


            Player player = equipment.player;
            var items = Unturnov.Database.GetItems(oItem.TableName, storageId, out byte storedHeight, out byte storedWidth);

            // add stored items
            foreach (ItemJar jar in items)
            {
                if (oItem.Height >= storedHeight && oItem.Width >= storedWidth)
                {
                    storage.addItem(jar.x, jar.y, jar.rot, jar.item);
                }
                else if (CanAdd(jar, oItem.Height, oItem.Width))
                {
                    storage.addItem(jar.x, jar.y, jar.rot, jar.item);
                }
            }

            storage.onItemAdded += (page, index, jar) =>
            {
                if (storage.getItemCount() < 200) return;
                Item item = storage.getItem(index).item;
                ItemManager.dropItem(item, player.character.position, true, true, true);
                storage.removeItem(index);
            };

            player.inventory.isStoring = true;
            player.inventory.isStorageTrunk = false;
            player.inventory.updateItems(PlayerInventory.STORAGE, storage);
            player.inventory.sendStorage();

            CSteamID steamID = UnturnedPlayer.FromPlayer(player).CSteamID;
            if (!OpenItems.ContainsKey(steamID))
            {
                OpenItems.Add(UnturnedPlayer.FromPlayer(player).CSteamID, new OpenedItem
                {
                    tableName = oItem.TableName,
                    storageId = storageId,
                    stroage = storage,
                    openableItemId = oItem.Id,
                });
            }


            return true;
        }

        #region HelperFunctions
        public static bool isItemStorageOpen(UnturnedPlayer player)
        {
            if (OpenItems.TryGetValue(player.CSteamID, out OpenedItem oItem))
            {
                if (!oItem.stroage.Equals(player.Inventory.items[(byte)InventoryGroup.Storage]))
                {
                    Unturnov.Database.SetItems(oItem.tableName, ref oItem.storageId, oItem.stroage);
                    OpenItems.Remove(player.CSteamID);
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public static void notifyNotAllowed(UnturnedPlayer player, ushort Id)
        {
            if (Unturnov.Conf.OpenableItemsConfig.Notification_UI.Enabled)
            {
                EffectControler.spawnUI(Unturnov.Conf.OpenableItemsConfig.Notification_UI.UI_Id, Unturnov.Conf.OpenableItemsConfig.Notification_UI.UI_Key, player.CSteamID);
            }
            else
            {
                UnturnedChat.Say(player, Util.Translate("item_restricted", Assets.find(EAssetType.ITEM, Id).name), Color.red);
            }
        }
        internal static bool isWhitelisted(UnturnedPlayer uPlayer, ushort Id)
        {
            if (uPlayer != null && OpenItems.TryGetValue(uPlayer.CSteamID, out OpenedItem oItem))
            {
                if (WhitelistedItems.TryGetValue(Id, out List<ushort> openableItemIds) && openableItemIds.Contains(oItem.openableItemId))
                {
                    return true;
                }
            }
            return false;
        }
        internal static uint convertStorageId(ItemAsset asset, byte[] state)
        {
            int i = asset.getState().Length;
            byte[] s = new byte[]{
                    state[i++],
                    state[i++],
                    state[i++],
                    0,
                };

            return BitConverter.ToUInt32(s, 0);
        }
        internal static byte[] convertStorageId(uint storageId)
        {
            return BitConverter.GetBytes(storageId);
        }
        private static bool CanAdd(ItemJar jar, byte height, byte width)
        {
            var sizeX = jar.rot % 2 == 1 ? jar.size_y : jar.size_x;
            var sizeY = jar.rot % 2 == 1 ? jar.size_x : jar.size_y;

            return jar.x + sizeX - 1 < width && jar.y + sizeY - 1 < height;
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
        internal static Dictionary<ushort, List<ushort>> createWhitelistDictionary(List<OpenableItem> openableItems)
        {
            Dictionary<ushort, List<ushort>> whithelistDict = new Dictionary<ushort, List<ushort>>();
            if (openableItems != null)
            {
                // check all openable items
                foreach (OpenableItem oItem in openableItems)
                {
                    if (oItem.Id == 0)
                    {
                        Logger.Log("Openable item with Id:" + oItem.Id + " was skipped!");
                        continue;
                    }
                    if (oItem.UsedWhitelists != null)
                    {
                        // check all connected whitelists
                        foreach (string whiltelistName in oItem.UsedWhitelists)
                        {
                            if (!tryGetWhitelist(whiltelistName, out List<ItemExtension> items))
                            {
                                Logger.LogWarning("Item Whitelist with Name:" + whiltelistName + " was not found!");
                            }
                            else
                            {
                                // add all whitelisted items and the connected openable item to the dict
                                foreach (ItemExtension item in items)
                                {
                                    if (whithelistDict.TryGetValue(item.Id, out List<ushort> openableItemIds))
                                    {
                                        openableItemIds.Add(oItem.Id);
                                    }
                                    else
                                    {
                                        whithelistDict.Add(item.Id, new List<ushort> { oItem.Id });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return whithelistDict;
        }
        internal static bool tryGetWhitelist(string name, out List<ItemExtension> whitelist)
        {
            whitelist = new List<ItemExtension>();
            foreach (ItemWhitelist list in Unturnov.Conf.OpenableItemsConfig.ItemWhitelists)
            {
                if (list.Name.Equals(name))
                {
                    whitelist = list.WhitelistedItems;
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
