using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    public class SecureCaseControler
    {
        public static SecureCaseConfig Conf { get; private set; }
        public static Dictionary<ulong, CaseContent> storedPlayerItems = new Dictionary<ulong, CaseContent>();

        internal static void Init(SecureCaseConfig conf)
        {
            Conf = conf;
        }

        public static void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            if (flag.id == Conf.CaseUpgradeFlagId)
            {
                resizeHands(quests.player);
            }
        }
        public static void OnPrePlayerDead(PlayerLife playerLife)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(playerLife.player);

            CaseContent content = new CaseContent(
                player.CSteamID.m_SteamID,
                InventoryHelper.StorePage(player.Inventory.items[2])
                );

            if (storedPlayerItems.ContainsKey(player.CSteamID.m_SteamID))
            {
                storedPlayerItems.Remove(player.CSteamID.m_SteamID);
            }

            storedPlayerItems.Add(player.CSteamID.m_SteamID, content);

            InventoryHelper.clearInventoryPage(player, 2);
            if (Conf.Debug)
            {
                Logger.Log("saved " + content.Items.Count() + " Items");
            }
        }
        public static void OnPlayerRevived(PlayerLife playerLife)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(playerLife.player);
            if (storedPlayerItems.TryGetValue(player.CSteamID.m_SteamID, out CaseContent content))
            {
                resizeHands(player.Player);
                InventoryHelper.RestorePage(player.Inventory, player.Inventory.items[2], content.Items);
                storedPlayerItems.Remove(player.CSteamID.m_SteamID);
            }
        }
        public static void OnPlayerConnected(UnturnedPlayer player)
        {
            List<StoredItem> storedItems;
            if (storedPlayerItems.TryGetValue(player.CSteamID.m_SteamID, out CaseContent content))
            {
                storedItems = content.Items;
                storedPlayerItems.Remove(player.CSteamID.m_SteamID);
            }
            else
            {
                storedItems = InventoryHelper.StorePage(player.Inventory.items[2]);
            }

            InventoryHelper.clearInventoryPage(player, 2);
            resizeHands(player.Player);
            InventoryHelper.RestorePage(player.Inventory, player.Inventory.items[2], storedItems);
        }
        public static void OnItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(inventory.player);

            // Hand check
            if (page_0 != 2 && page_1 != 2 || page_0 == 2 && page_1 == 2)
                return;

            if (Conf.Debug)
                Logger.Log("Swapped Item to Hands");

            byte otherPage = page_0;
            byte added_x = x_0;
            byte added_y = y_0;

            if (page_1 != 2)
            {
                otherPage = page_1;
                added_x = x_1;
                added_y = y_1;
            }

            var index = inventory.items[otherPage].getIndex(added_x, added_y);
            if (index == byte.MaxValue)
                return;


            var itemJar = inventory.items[otherPage].getItem(index);

            if (isBlacklisted(itemJar.item.id))
            {
                shouldAllow = false;
                notifyNotAllowed(player, itemJar.item.id);
            }
        }
        public static void OnItemDragged(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(inventory.player);

            // Hand check
            if (page_1 != 2)
                return;


            if (Conf.Debug)
                Logger.Log("Dragged Item to Hands");

            var index = inventory.getIndex(page_0, x_0, y_0);
            if (index == byte.MaxValue || page_1 >= PlayerInventory.PAGES - 1 || inventory.items[page_1] == null ||
                inventory.getItemCount(page_1) >= 200)
                return;

            var itemJar = inventory.items[page_0].getItem(index);
            if (itemJar == null || !inventory.checkSpaceDrag(page_1, x_0, y_0, itemJar.rot, x_1, y_1, rot_1,
                itemJar.size_x, itemJar.size_y, page_0 == page_1))
                return;

            if (isBlacklisted(itemJar.item.id))
            {
                ItemJar itemJ = inventory.getItem(page_0, index);
                if (page_0 == (byte)InventoryGroup.Storage && tryAddItem(player, itemJ.item))
                {
                    inventory.removeItem(page_0, index);
                }
                else
                {
                    shouldAllow = false;
                    notifyNotAllowed(player, itemJ.item.id);
                }
            }
        }
        public static void OnTakeItem(Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            if (isBlacklisted(itemData.item.id) && to_page == (byte)InventoryGroup.Hands)
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
                shouldAllow = false;
                notifyNotAllowed(uPlayer, itemData.item.id);
            }
        }
        public static void OnAddItem(PlayerInventory inventory, Items page, Item item, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(inventory.player);

            if (page.page == (byte)InventoryGroup.Hands && isBlacklisted(item.id))
            {
                if (player != null)
                {
                    shouldAllow = false;
                    return;
                }
                Logger.LogError("Add patch not working");
            }
        }

        #region Helper Functions
        public static bool tryAddItem(UnturnedPlayer player, Item item)
        {
            bool addedItem = false;
            byte page = 3;
            while (!addedItem && page < 7)
            {
                addedItem = player.Inventory.tryAddItem(item, 255, 255, page, 0);
                page++;
            }
            if (addedItem && Conf.Debug)
            {
                Logger.Log("Item " + item + " was added to page: " + page);
            }
            return addedItem;
        }
        public static void resizeHands(Player player)
        {
            if (Conf.CaseSizes.Count < 1)
            {
                Logger.LogError("No CaseSizes Defined!");
                return;
            }
            short caseLvl;
            if (!player.quests.getFlag(Conf.CaseUpgradeFlagId, out caseLvl) || caseLvl < 0)
            {
                caseLvl = 0;
            }
            if (caseLvl >= Conf.CaseSizes.Count)
            {
                caseLvl = (short)(Conf.CaseSizes.Count - 1);
            }

            player.inventory.items[2].resize(Conf.CaseSizes[caseLvl].Width, Conf.CaseSizes[caseLvl].Height);
        }
        public static bool isBlacklisted(ushort itemId)
        {
            return Conf.BlacklistedItems.Find(x => x.Id == itemId) != null;
        }
        public static void notifyNotAllowed(UnturnedPlayer player, ushort itemId)
        {
            if (Conf.Notification_UI.Enabled)
            {
                EffectControler.spawnUI(Conf.Notification_UI.UI_Id, Conf.Notification_UI.UI_Key, player.CSteamID);
            }
            else
            {
                UnturnedChat.Say(player, Util.Translate("item_restricted", Assets.find(EAssetType.ITEM, itemId).name), Color.red);
            }
        }

        public static void RestoreHands(UnturnedPlayer player, CaseContent content)
        {
            InventoryHelper.RestorePage(player.Inventory, player.Inventory.items[2], content.Items);
        }
        #endregion
    }
}
