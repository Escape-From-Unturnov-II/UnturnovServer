using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Controlers
{
    internal class DeathAdditionsControler
    {
        public static DeathAdditionConfig Conf;
        private static Dictionary<CSteamID, SavedPlayerKit> storedPlayerKits = new Dictionary<CSteamID, SavedPlayerKit>();
        internal static void Init(DeathAdditionConfig conf)
        {
            Conf = conf;
        }
        internal static void OnPrePlayerDead(PlayerLife playerLife)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(playerLife.player);

            if (!storedPlayerKits.ContainsKey(uPlayer.CSteamID)) 
            { 
                storedPlayerKits.Add(uPlayer.CSteamID, new SavedPlayerKit());
            }

            SavedPlayerKit kit = storedPlayerKits[uPlayer.CSteamID];

            kit.health = playerLife.health;
            kit.food = playerLife.food;
            kit.water = playerLife.water;
            kit.virus = playerLife.virus;

        }
        internal static void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            //TODO: spawn Death UI (ID 52313)
            if (ScavRunControler.isScavRunActive(player))
            {
                return;
            }
            checkClothingDrops(player);
            checkDeathDrops(player, cause);
        }
        internal static void OnPlayerRevived(PlayerLife playerLife)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(playerLife.player);
            if (storedPlayerKits.TryGetValue(player.CSteamID, out SavedPlayerKit storedPlayerKit))
            {
                if (storedPlayerKit == null) return;

                checkKeepStats(playerLife, storedPlayerKit);

                foreach (Item item in storedPlayerKit.kitItems)
                {
                    player.Player.inventory.tryAddItem(item, true, false);
                }
                if (Conf.Debug)
                {
                    Logger.Log($"Restored {storedPlayerKit.kitItems.Count} items on revive");
                }
                storedPlayerKits.Remove(player.CSteamID);
            }
        }
        private static void checkDeathDrops(UnturnedPlayer player, EDeathCause cause)
        {
            if (cause != EDeathCause.SUICIDE)
            {

                if (Conf.DeathDrops?.Count > 0)
                {
                    // defaults to index 0 if no flag it set or found
                    Item item = new Item(Conf.DeathDrops[0].Id, true);
                    if (Conf.DeathDropFlag != 0 && player.Player.quests.getFlag(Conf.DeathDropFlag, out short dropFlagValue))
                    {
                        DeathDrop drop = Conf.DeathDrops.Find(x => x.RequiredFalgValue == dropFlagValue);
                        if (drop != null)
                        {
                            item = new Item(drop.Id, true);
                        }
                    }
                    if (Conf.Debug)
                    {
                        Logger.Log($"deathdrop {item.id} dropped");
                    }
                    ItemManager.dropItem(item, player.Position, true, false, true);
                }
            }
        }
        private static void checkClothingDrops(UnturnedPlayer player)
        {
            List<Item> storedClothing = new List<Item>();
            PlayerClothing clothing = player.Player.clothing;
            if (!Conf.DropBackpack)
            {
                if(clothing.thirdClothes.backpack != 0)
                {
                    storedClothing.Add(new Item(clothing.backpack, 1, clothing.backpackQuality, clothing.backpackState));
                    clothing.thirdClothes.backpack = 0;
                    clothing.askWearBackpack(0, 0, new byte[0], true);
                }
            }
            if (!Conf.DropVest)
            {
                if (clothing.thirdClothes.vest != 0)
                {
                    storedClothing.Add(new Item(clothing.vest, 1, clothing.vestQuality, clothing.vestState));
                    clothing.thirdClothes.vest = 0;
                    clothing.askWearVest(0, 0, new byte[0], true);
                }
            }
            if (!Conf.DropShirt)
            {
                if (clothing.thirdClothes.shirt != 0)
                {
                    storedClothing.Add(new Item(clothing.shirt, 1, clothing.shirtQuality, clothing.shirtState));
                    player.Player.clothing.thirdClothes.shirt = 0;
                    player.Player.clothing.askWearShirt(0, 0, new byte[0], true);
                }
            }
            if (!Conf.DropPants)
            {
                if (clothing.thirdClothes.pants != 0)
                {
                    storedClothing.Add(new Item(clothing.pants, 1, clothing.pantsQuality, clothing.pantsState));
                    player.Player.clothing.thirdClothes.pants = 0;
                    player.Player.clothing.askWearPants(0, 0, new byte[0], true);
                }
            }
            if (!Conf.DropGlasses)
            {
                if (clothing.thirdClothes.glasses != 0)
                {
                    storedClothing.Add(new Item(clothing.glasses, 1, clothing.glassesQuality, clothing.glassesState));
                    player.Player.clothing.thirdClothes.glasses = 0;
                    player.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
                }
            }
            if (!Conf.DropHat)
            {
                if (clothing.thirdClothes.hat != 0)
                {
                    storedClothing.Add(new Item(clothing.hat, 1, clothing.hatQuality, clothing.hatState));
                    player.Player.clothing.thirdClothes.hat = 0;
                    player.Player.clothing.askWearHat(0, 0, new byte[0], true);
                }
            }
            if (!Conf.DropMask)
            {
                if (clothing.thirdClothes.mask != 0)
                {
                    storedClothing.Add(new Item(clothing.mask, 1, clothing.maskQuality, clothing.maskState));
                    player.Player.clothing.thirdClothes.mask = 0;
                    player.Player.clothing.askWearMask(0, 0, new byte[0], true);
                }
            }

            if (Conf.Debug) 
            {
                Logger.Log($"Stored {storedClothing.Count} clothing items on death");
            }
            if (storedPlayerKits.ContainsKey(player.CSteamID))
            {
                storedPlayerKits[player.CSteamID].kitItems = storedClothing;
            }
            else
            {
                storedPlayerKits.Add(player.CSteamID, new SavedPlayerKit(storedClothing));
            }
        }

        private static void checkKeepStats(PlayerLife playerLife, SavedPlayerKit storedPlayerKit)
        {
            if (Conf.KeepFood)
            {
                playerLife.serverModifyFood(storedPlayerKit.food);
            }
            if (Conf.KeepWater)
            {
                playerLife.serverModifyWater(storedPlayerKit.water);
            }
            if (Conf.KeepVirus)
            {
                playerLife.serverModifyVirus(storedPlayerKit.virus);
            }
        }
    }
}
