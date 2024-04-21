using Rocket.Core.Assets;
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
using static SpeedMann.Unturnov.Models.GunAttachments;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Controlers
{
    internal class WeaponModdingControler
    {
        private static bool LogDebug = false;
        private static Dictionary<CSteamID, GunAttachments> ModdedGunAttachments;
        private static Dictionary<ushort, ItemExtension> GunModdingDict;

        internal static void Init(List<ItemExtension> GunModdingExtensions, bool Debug = false)
        {
            LogDebug = Debug;
            GunModdingDict = Unturnov.createDictionaryFromItemExtensions(GunModdingExtensions);
            ModdedGunAttachments = new Dictionary<CSteamID, GunAttachments>();
        }
        internal static void PreventAutoEquipOfCraftedGuns(PlayerInventory inventory, Item item, ref bool autoEquipWeapon, ref bool autoEquipUseable, ref bool autoEquipClothing)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(inventory.player);
            if (GunModdingDict.ContainsKey(item.id) && ModdedGunAttachments.ContainsKey(player.CSteamID))
            {
                autoEquipClothing = false;
                autoEquipUseable = false;
                autoEquipWeapon = false;
            }
        }
        internal static void HandleAttachmentsOfCraftedGuns(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            if (!GunModdingDict.ContainsKey(P.item.id) || !ModdedGunAttachments.TryGetValue(player.CSteamID, out GunAttachments attachments))
                return;

            ModdedGunAttachments.Remove(player.CSteamID);
            ItemGunAsset gunAsset = Assets.find(EAssetType.ITEM, P.item.id) as ItemGunAsset;
            if (gunAsset == null)
                return;

            byte[] newState = getCleanInitialState(gunAsset);

            checkAttachments(gunAsset, attachments, ref newState);
            checkMagazine(gunAsset, attachments, ref newState);
            checkIncompatible(player, attachments);

            player.Inventory.sendUpdateInvState((byte)inventoryGroup, P.x, P.y, newState);
        }
        internal static void SaveAttachmentsOfCraftedGun(Blueprint blueprint, PlayerCrafting crafting)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);

            GunAttachments attachments = tryGetAndRemoveAttachmentsOfCraftedGun(player, blueprint);

            if (attachments == null)
                return;

            // save attachments
            if (!ModdedGunAttachments.ContainsKey(player.CSteamID))
            {
                ModdedGunAttachments.Add(player.CSteamID, attachments);
            }
            else
            {
                ModdedGunAttachments[player.CSteamID] = attachments;
            }
        }

        #region HelperFunctions
        private static GunAttachments tryGetAndRemoveAttachmentsOfCraftedGun(UnturnedPlayer player, Blueprint blueprint)
        {
            foreach (BlueprintSupply supply in blueprint.supplies)
            {
                if (!GunModdingDict.ContainsKey(supply.id))
                    continue;

                List<InventorySearch> itemList = player.Inventory.search(supply.id, true, true);
                if (itemList.Count <= 0)
                    continue;

                ItemGunAsset asset = Assets.find(EAssetType.ITEM, itemList[0].jar.item.id) as ItemGunAsset;
                if (asset == null)
                    continue;

                GunAttachments attachments = new GunAttachments(itemList[0].jar.item.metadata);

                byte index = player.Inventory.findIndex(itemList[0].page, itemList[0].jar.x, itemList[0].jar.y, out byte found_x, out byte found_y);
                player.Inventory.updateState(itemList[0].page, index, new byte[18]);

                if (LogDebug)
                {
                    Logger.Log($"Modded weapon with: sight {attachments.attachments[0].id}, tactical {attachments.attachments[1].id}, grip {attachments.attachments[2].id}, barrel {attachments.attachments[3].id}, mag {attachments.magAttachment.id}, ammo {attachments.ammo}");
                }

                return attachments;
            }
            return null;
        }
        private static void checkAttachments(ItemGunAsset gunAsset, GunAttachments attachments, ref byte[] state)
        {
            foreach (ushort caliber in gunAsset.attachmentCalibers)
            {
                foreach (GunAttachment att in attachments.attachments)
                {
                    if (!att.wasSet && att.calibers != null && att.calibers.Contains(caliber))
                    {
                        att.SetAttachment(ref state);
                    }
                }
            }
        }
        private static void checkMagazine(ItemGunAsset gunAsset, GunAttachments attachments, ref byte[] state)
        {
            foreach (ushort caliber in gunAsset.magazineCalibers)
            {
                if (attachments.magAttachment.calibers != null && attachments.magAttachment.calibers.Contains(caliber))
                {
                    attachments.magAttachment.SetAttachment(ref state);
                    break;
                }
            }
        }
        private static byte[] getCleanInitialState(ItemGunAsset gunAsset)
        {
            if(gunAsset == null)
                return new byte[0];

            // get initial state and remove mag and ammo
            byte[] newState = gunAsset.getState();
            newState[8] = 0;
            newState[9] = 0;
            newState[10] = 0;
            return newState;
        }
        private static void checkIncompatible(UnturnedPlayer player, GunAttachments attachments)
        {
            foreach (GunAttachment att in attachments.attachments)
            {
                if (!att.wasSet && att.id != 0)
                {
                    Item item = new Item(att.id, true);
                    giveIncompatible(player, item);
                }
            }

            if (!attachments.magAttachment.wasSet && attachments.magAttachment.id != 0)
            {
                Item item = new Item(attachments.magAttachment.id, attachments.ammo, 100);
                giveIncompatible(player, item);
            }
        }
        private static void giveIncompatible(UnturnedPlayer player, Item item)
        {
            if (LogDebug)
            {
                Logger.Log($"gave incompatible item: {item.id} after weapon crafting");
            }
            if (!player.GiveItem(item))
            {
                player.Inventory.forceAddItem(item, false);
            }
        }
        #endregion
    }
}
