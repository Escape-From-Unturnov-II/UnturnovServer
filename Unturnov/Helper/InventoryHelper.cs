using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using MySqlX.XDevAPI.Relational;
using Rocket.API;
using Rocket.Core.Assets;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;

namespace SpeedMann.Unturnov
{
	public class InventoryHelper
	{
		public static ItemJar getItemJarOfEquiped(PlayerEquipment equipment)
        {
			if(equipment != null)
            {
				byte page = equipment.equippedPage;
				byte x = equipment.equipped_x;
				byte y = equipment.equipped_y;
				byte index = equipment.player.inventory.findIndex(page, x, y, out byte found_x, out byte found_y);
				if (index != 255)
				{
					return equipment.player.inventory.getItem(page, index);
				}
			}
			return null;
		}
		public static void setMagForGun(PlayerEquipment gun, Item mag)
		{
			if (gun != null && gun.state.Length >= 18 && gun.useable is UseableGun)
			{
				byte[] array = BitConverter.GetBytes(mag.id);
				gun.state[8] = array[0];
				gun.state[9] = array[1];
				gun.state[10] = mag.amount;
				gun.state[17] = mag.quality;
				gun.sendUpdateState();
			}
		}
		public static void setMagForGun(Item gun, Item mag)
		{
			if (gun != null && gun.state.Length >= 18)
			{
				byte[] array = BitConverter.GetBytes(mag.id);
				gun.state[8] = array[0];
				gun.state[9] = array[1];
				gun.state[10] = mag.amount;
				gun.state[17] = mag.quality;
			}
		}
		public static bool IsCompatible(ItemGunAsset itemGunAsset, ItemMagazineAsset itemMagazineAsset)
		{
			if(itemGunAsset == null || itemMagazineAsset == null)
			{
				return false;
			}

            foreach (byte magCalliber in itemMagazineAsset.calibers)
            {
                foreach (byte gunMagCalliber in itemGunAsset.magazineCalibers)
                {
                    if (magCalliber == gunMagCalliber)
                    {
						return true;
                    }
                }
            }
			return false;
        }
		public static Item getMagFromGun(PlayerEquipment gun)
		{
			if (gun != null && gun.state.Length >= 18 && gun.useable is UseableGun)
			{
                byte[] mag = new byte[] { gun.state[8], gun.state[9] };
                ushort itemId = BitConverter.ToUInt16(mag, 0);
                if (itemId > 0)
                {
                    return new Item(itemId, gun.state[10], gun.state[17]);
                }
            }
            return null;
        }
        public static Item getMagFromGun(Item gun)
		{
			if (gun != null && gun.state.Length >= 18)
			{
				byte[] mag = new byte[] { gun.state[8], gun.state[9] };
				ushort itemId = BitConverter.ToUInt16(mag, 0);
                if (itemId > 0)
                {
					return new Item(itemId, gun.state[10], gun.state[17]);
				}
            }
			return null;
        }
        public static void getAttachments(byte[] state, out ushort sight, out ushort tactical, out ushort grip, out ushort barrel, out ushort magazine)
		{
            Attachments.parseFromItemState(state, out sight, out tactical, out grip, out barrel, out magazine);
        }
		public static void removeMagFromGun(Item gun)
		{
			if(gun != null && gun.state.Length >= 18)
            {
				gun.state[8] = 0;
				gun.state[9] = 0;
				gun.state[10] = 0;
				gun.state[17] = 0;
			}
		}
		public static void removeMagFromGun(PlayerEquipment gun)
		{
			if (gun != null && gun.state.Length >= 18)
			{
				gun.state[8] = 0;
				gun.state[9] = 0;
				gun.state[10] = 0;
				gun.state[17] = 0;
				gun.sendUpdateState();
			}
		}
		public static void clearInventoryPage(UnturnedPlayer player, byte page)
		{
			if (player.Player.inventory.items[page] == null)
				return;
			while(player.Player.inventory.getItemCount(page) > 0)
            {
				player.Player.inventory.removeItem(page, 0);
			}
		}
		public static bool itemEquality(Item a, Item b)
		{
			bool equals = false;
			ItemAsset itemAssetA = Assets.find(EAssetType.ITEM, a.id) as ItemAsset;
			ItemAsset itemAssetB = Assets.find(EAssetType.ITEM, b.id) as ItemAsset;
			if (itemAssetA is ItemGunAsset && itemAssetB is ItemGunAsset)
			{
				equals = a != null && b != null
								&& a.id == b.id
								&& a.durability == b.durability
								&& a.quality == b.quality
								&& a.amount == b.amount
								&& a.state[0] == b.state[0] // Sight 1
								&& a.state[1] == b.state[1] // Sight 2
								&& a.state[2] == b.state[2] // Tactical 1
								&& a.state[3] == b.state[3] // Tactical 2
								&& a.state[4] == b.state[4] // Grip 1
								&& a.state[5] == b.state[5] // Grip 2
								&& a.state[6] == b.state[6] // Barrel 1
								&& a.state[7] == b.state[7] // Barrel 2
								&& a.state[8] == b.state[8] // Mag 1
								&& a.state[9] == b.state[9] // Mag 2
								&& a.state[10] == b.state[10]; // Mag Ammo
			}
			else
			{
				equals = a != null && b != null
								&& a.id == b.id
								&& a.durability == b.durability
								&& a.quality == b.quality
								&& a.amount == b.amount;
			}

			return equals;


		}
        public static void stackItem(PlayerInventory inventory, ItemJar itemJar_0, byte page_0, ItemJar itemJar_1, byte page_1, byte index_1, byte stackSize)
        {
            int newAmmount = itemJar_0.item.amount + itemJar_1.item.amount;

            if (newAmmount > stackSize)
            {
                inventory.sendUpdateAmount(page_0, itemJar_0.x, itemJar_0.y, stackSize);
                inventory.sendUpdateAmount(page_1, itemJar_1.x, itemJar_1.y, (byte)(newAmmount - stackSize));
                Logger.Log($"Filled stack of {itemJar_0.item.id} {stackSize}");
            }
            else
            {
                inventory.removeItem(page_1, index_1);
                inventory.sendUpdateAmount(page_0, itemJar_0.x, itemJar_0.y, (byte)newAmmount);
                Logger.Log($"Set new amount {newAmmount} stack of {itemJar_0.item.id} with stacksize: {stackSize}");
            }
        }
        public static int searchAmount(PlayerInventory inventory, out List<InventorySearch> search, ushort id)
		{
            search = new List<InventorySearch>();
			int foundAmmount = 0;
            for (byte pageIndex = PlayerInventory.SLOTS; pageIndex < PlayerInventory.PAGES - 2; pageIndex++)
            {
                var pageItems = inventory.items[pageIndex].items;
                for (byte i = 0; i < pageItems.Count; i++)
                {
                    ItemJar itemJar = pageItems[i];
                    if (itemJar.item.id != id 
						|| itemJar.item.amount < 0)
                    {
						continue;
                    }
                    ItemAsset asset = itemJar.GetAsset();
                    if (asset == null)
                    {
                        continue;
                    }
                    search.Add(new InventorySearch(pageIndex, itemJar));
                    foundAmmount += itemJar.item.amount;
                }
            }
			return foundAmmount;
        }
		public static bool tryFindSplittable(PlayerInventory inventory, out InventorySearch search, ushort id)
		{
			search = null;
            for (byte pageIndex = PlayerInventory.SLOTS; pageIndex < PlayerInventory.PAGES - 2; pageIndex++)
            {
                var pageItems = inventory.items[pageIndex].items;
                for (byte i = 0; i < pageItems.Count; i++)
                {
                    ItemJar itemJar = pageItems[i];
                    if (itemJar.item.id != id
                        || itemJar.item.amount < 1)
                    {
                        continue;
                    }
                    ItemAsset asset = itemJar.GetAsset();
                    if (asset == null)
                    {
                        continue;
                    }
                    search = new InventorySearch(pageIndex, itemJar);
					return true;
                }
            }
            return false;
        }
        public static ushort findAmmo(PlayerInventory inventory, ushort itemId, out List<InventorySearch> foundAmmo)
		{
            ushort amount = 0;
            foundAmmo = inventory.search(itemId, false, true);
            foreach (InventorySearch search in foundAmmo)
            {
                amount += search.jar.item.amount;
            }
			return amount;
        }
        public static void forceAddItem(PlayerInventory inventory, Item item, byte x, byte y, byte page, byte rot)
		{
            if (!inventory.tryAddItem(item, x, y, page, rot))
            {
                inventory.forceAddItem(item, false);
            }
        }
        public static bool tryAddItem(UnturnedPlayer player, Item item, byte startPage, byte endPage = 6)
        {
			if (endPage > 6)
				endPage = 6;

            bool addedItem = false;
            byte page = startPage;
            while (!addedItem && page <= endPage)
            {
                addedItem = player.Inventory.tryAddItem(item, 255, 255, page, 0);
                page++;
            }

            return addedItem;
        }
        public static bool getInvItems(UnturnedPlayer player, ref List<ItemJarWrapper> foundItems)
		{
			bool returnv = false;
			if (foundItems == null) return returnv;

			try
			{
				for (byte i = 0; i < player.Inventory.items.Length; i++)
				{
					if (player.Inventory.items[i] == null) continue;

					for (byte y = 0; y < player.Inventory.items[i].items.Count; y++)
					{
						ItemJar itemJ = player.Inventory.items[i].items[y];
						foundItems.Add(new ItemJarWrapper(itemJ, i, y));
					}
					returnv = true;
				}

			}
			catch (Exception e)
			{
				Logger.Log("There was an error getting items from " + player.DisplayName + "'s inventory.  Here is the error.");
				Console.Write(e);
			}
			return returnv;
		}
		public static bool getClothingItems(UnturnedPlayer player, ref List<KeyValuePair<StorageType, Item>> foundItems)
		{
			bool returnv = false;
			if (foundItems == null) return returnv;

			try
			{
                List<ItemClothingAsset> clothing = new List<ItemClothingAsset>
                {
                    player.Player.clothing.hatAsset,
                    player.Player.clothing.maskAsset,
                    player.Player.clothing.glassesAsset,
                    player.Player.clothing.backpackAsset,
                    player.Player.clothing.vestAsset,
                    player.Player.clothing.shirtAsset,
                    player.Player.clothing.pantsAsset
                };

                foreach (ItemClothingAsset item in clothing)
				{
					if (item != null)
					{
						byte quality = 0;
						byte[] state = new byte[0];
						StorageType type = StorageType.Unknown;

						switch (item)
						{
							case ItemHatAsset hat:
								quality = player.Player.clothing.hatQuality;
								state = player.Player.clothing.hatState;
								type = StorageType.Hat;
								break;
							case ItemMaskAsset mask:
								quality = player.Player.clothing.maskQuality;
								state = player.Player.clothing.maskState;
								type = StorageType.Mask;
								break;
							case ItemGlassesAsset glasses:
								quality = player.Player.clothing.glassesQuality;
								state = player.Player.clothing.glassesState;
								type = StorageType.Glasses;
								break;
							case ItemBackpackAsset backpack:
								quality = player.Player.clothing.backpackQuality;
								state = player.Player.clothing.backpackState;
								type = StorageType.Backpack;
								break;
							case ItemVestAsset vest:
								quality = player.Player.clothing.vestQuality;
								state = player.Player.clothing.vestState;
								type = StorageType.Vest;
								break;
							case ItemShirtAsset shirt:
								quality = player.Player.clothing.shirtQuality;
								state = player.Player.clothing.shirtState;
								type = StorageType.Shirt;
								break;
							case ItemPantsAsset pants:
								quality = player.Player.clothing.pantsQuality;
								state = player.Player.clothing.pantsState;
								type = StorageType.Pants;
								break;
						}
						foundItems.Add(new KeyValuePair<StorageType, Item>(type, new Item(item.id, 1, quality, state)));
					}
				}
				returnv = true;
			}
			catch (Exception e)
			{
				Logger.Log("There was an error getting clothes from " + player.DisplayName + "'.  Here is the error.");
				Console.Write(e);
			}
			return returnv;
		}
		public static bool clearAll(UnturnedPlayer player)
		{
			return clearInv(player) && clearClothes(player);
		}
		public static bool clearInv(UnturnedPlayer player)
		{
			bool returnv = false;
			try
			{
				player.Player.equipment.dequip();
				for (byte p = 0; p < PlayerInventory.PAGES; p++)
				{
					if (player.Inventory.items[p] == null) continue;
					byte itemc = player.Player.inventory.getItemCount(p);
					if (itemc > 0)
					{
						for (byte p1 = 0; p1 < itemc; p1++)
						{
							player.Player.inventory.removeItem(p, 0);
						}
					}
				}

				player.Player.inventory.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
				{
					(byte)0,
					(byte)0,
					new byte[0]
				});
				player.Player.inventory.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
				{
					(byte)1,
					(byte)0,
					new byte[0]
				});
				returnv = true;
			}
			catch (Exception e)
			{
				Logger.Log($"There was an error clearing {player.DisplayName}'s inventory.  Here is the error.");
				Console.Write(e);
			}
			return returnv;
		}
		public static bool clearClothes(UnturnedPlayer player)
		{
			bool returnv = false;

			byte oldWidth = player.Inventory.items[2].width;
			byte oldHeight = player.Inventory.items[2].height;

			player.Inventory.items[2].resize(10, 10);
			
			try
			{
				player.Player.clothing.askWearBackpack(0, 0, new byte[0], true);
				for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
				{
					player.Player.inventory.removeItem(2, 0);
				}
				player.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
				for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
				{
					player.Player.inventory.removeItem(2, 0);
				}
				player.Player.clothing.askWearHat(0, 0, new byte[0], true);
				for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
				{
					player.Player.inventory.removeItem(2, 0);
				}
				player.Player.clothing.askWearMask(0, 0, new byte[0], true);
				for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
				{
					player.Player.inventory.removeItem(2, 0);
				}
				player.Player.clothing.askWearPants(0, 0, new byte[0], true);
				for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
				{
					player.Player.inventory.removeItem(2, 0);
				}
				player.Player.clothing.askWearShirt(0, 0, new byte[0], true);
				for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
				{
					player.Player.inventory.removeItem(2, 0);
				}
				player.Player.clothing.askWearVest(0, 0, new byte[0], true);
				for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
				{
					player.Player.inventory.removeItem(2, 0);
				}
				returnv = true;
			}
			catch (Exception e)
			{
				Logger.Log("There was an error clearing " + player.DisplayName + "'s inventory.  Here is the error.");
				Console.Write(e);
			}
            finally
            {
				player.Inventory.items[2].resize(oldWidth, oldHeight);
			}
			return returnv;
		}
		public static List<StoredItem> StorePage(Items itemsPage)
		{
			List<StoredItem> newItems = new List<StoredItem>();

			foreach (ItemJar itemJ in itemsPage.items)
			{
				newItems.Add(new StoredItem(
					itemJ.item,
					itemJ.x,
					itemJ.y,
					itemJ.rot
					));
			}
			return newItems;
		}
		public static bool tryRestorePage(PlayerInventory inventory, Items itemsPage, List<StoredItem> storedItems)
		{
			if (storedItems == null)
				return false;

			int droppedItems = 0;
            foreach (StoredItem storedItem in storedItems)
			{
				Item item = new Item(storedItem.id, storedItem.amount, storedItem.quality, storedItem.state);
				if (!inventory.tryAddItem(item, storedItem.x, storedItem.y, itemsPage.page, storedItem.rot))
				{
					if (!inventory.tryAddItem(item, true))
					{
						droppedItems++;
                        ItemManager.dropItem(item, inventory.player.character.position, true, true, true);
					}
				}
			}

            if (Unturnov.Conf.Debug)
                Logger.Log($"Restored {storedItems.Count - droppedItems} items, dropped {droppedItems}");

			return true;
        }
        public static void FillExisting(PlayerInventory inventory, ushort id, byte stackSize, ref byte remainingAmount, out bool filledStack)
        {
            searchAmount(inventory, out List<InventorySearch> foundItems, id);
			FillExisting(inventory, foundItems, stackSize, ref remainingAmount, out filledStack);
        }
        public static void FillExisting(PlayerInventory inventory, List<InventorySearch> foundItems, byte stackSize, ref byte remainingAmount, out bool filledStack)
        {
            filledStack = false;
            foreach (InventorySearch foundItem in foundItems)
            {
                if (foundItem.jar.item.amount < stackSize)
                {
                    byte addedAmount = CalculateAddedAmount(remainingAmount, foundItem.jar.item.amount, stackSize);
                    remainingAmount -= addedAmount;
                    byte newAmount = (byte)(foundItem.jar.item.amount + addedAmount);
                    inventory.sendUpdateAmount(foundItem.page, foundItem.jar.x, foundItem.jar.y, newAmount);

                    if (foundItem.jar.item.amount == stackSize)
                    {
                        filledStack = true;
                    }
                }
            }
        }
        public static void ReplaceItem(PlayerInventory inventory, byte page, byte index, ushort replacementId)
		{
            inventory.removeItem(page, index);
            inventory.forceAddItem(new Item(replacementId, 1, 100), false);
        }
        public static void ReplaceItem(PlayerInventory inventory, InventorySearch itemToReplace, ushort replacementId)
        {
			byte index = inventory.getIndex(itemToReplace.page, itemToReplace.jar.x, itemToReplace.jar.y);
			ReplaceItem(inventory, itemToReplace.page, index, replacementId);
        }
        public static byte CalculateAddedAmount(byte availableAmount, byte currentAmount, byte stackSize)
        {
            byte remainingSpace = (byte)(stackSize - currentAmount);
            byte addedAmount = Math.Min(availableAmount, remainingSpace);
            return addedAmount;
        }
        public enum StorageType
		{
			PrimaryWeapon = 0,
			SecondaryWeapon = 1,
			Hands = 2,
			Backpack = 3,
			Vest = 4,
			Shirt = 5,
			Pants = 6,
			Hat = 7,
			Mask = 8,
			Glasses = 9,
			Unknown = 10,
		}
	}
}

