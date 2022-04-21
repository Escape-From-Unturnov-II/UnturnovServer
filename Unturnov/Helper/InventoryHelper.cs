using System;
using System.Collections.Generic;
using Rocket.API;
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
		public static void clearInventoryPage(UnturnedPlayer player, byte page)
		{
			if (player.Player.inventory.items[page] == null)
				return;

			for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(page); p2++)
			{
				player.Player.inventory.removeItem(page, 0);
			}
		}
		public static bool ItemEquality(Item a, Item b)
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

		public static bool GetInvItems(UnturnedPlayer player, ref List<ItemJarWrapper> foundItems)
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
		public static bool GetClothingItems(UnturnedPlayer player, ref List<KeyValuePair<StorageType, Item>> foundItems)
		{
			bool returnv = false;
			if (foundItems == null) return returnv;

			try
			{
				List<ItemClothingAsset> clothing = new List<ItemClothingAsset>();

				clothing.Add(player.Player.clothing.hatAsset);
				clothing.Add(player.Player.clothing.maskAsset);
				clothing.Add(player.Player.clothing.glassesAsset);
				clothing.Add(player.Player.clothing.backpackAsset);
				clothing.Add(player.Player.clothing.vestAsset);
				clothing.Add(player.Player.clothing.shirtAsset);
				clothing.Add(player.Player.clothing.pantsAsset);

				foreach (ItemClothingAsset item in clothing)
				{
					if (item != null)
					{
						byte quality = 0;
						StorageType type = StorageType.Unknown;

						switch (item)
						{
							case ItemHatAsset hat:
								quality = player.Player.clothing.hatQuality;
								type = StorageType.Hat;
								break;
							case ItemMaskAsset mask:
								quality = player.Player.clothing.maskQuality;
								type = StorageType.Mask;
								break;
							case ItemGlassesAsset glasses:
								quality = player.Player.clothing.glassesQuality;
								type = StorageType.Glasses;
								break;
							case ItemBackpackAsset backpack:
								quality = player.Player.clothing.backpackQuality;
								type = StorageType.Backpack;
								break;
							case ItemVestAsset vest:
								quality = player.Player.clothing.vestQuality;
								type = StorageType.Vest;
								break;
							case ItemShirtAsset shirt:
								quality = player.Player.clothing.shirtQuality;
								type = StorageType.Shirt;
								break;
							case ItemPantsAsset pants:
								quality = player.Player.clothing.pantsQuality;
								type = StorageType.Pants;
								break;
						}
						foundItems.Add(new KeyValuePair<StorageType, Item>(type, new Item(item.id, true, quality)));
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

		public static bool ClearAll(UnturnedPlayer player)
		{
			return ClearInv(player) && ClearClothes(player);
		}
		public static bool ClearInv(UnturnedPlayer player)
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
				Logger.Log("There was an error clearing " + player.DisplayName + "'s inventory.  Here is the error.");
				Console.Write(e);
			}
			return returnv;
		}
		public static bool ClearClothes(UnturnedPlayer player)
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

