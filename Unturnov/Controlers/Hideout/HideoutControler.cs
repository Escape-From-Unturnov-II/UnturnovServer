using Newtonsoft.Json;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Controlers
{
    internal class HideoutControler
    {
        private static HideoutConfig Conf;
        private static Dictionary<CSteamID, List<BarricadeWrapper>> savedBarricades = new Dictionary<CSteamID, List<BarricadeWrapper>>();
        private static Dictionary<CSteamID, Hideout> claimedHideouts = new Dictionary<CSteamID, Hideout>();
        private static List<Hideout> freeHideouts = new List<Hideout>();
        private static string SaveFileName = "Hideout";

        internal static void Init(HideoutConfig hideoutConfig)
        {
            Conf = hideoutConfig;
            claimedHideouts = new Dictionary<CSteamID, Hideout>();
            freeHideouts.Add(new Hideout(new Vector3(868, 8.5f, -350), 0));
            //freeHideouts.Add(new Hideout(new Vector3(879, 8.5f, -350), 0));
            freeHideouts.Add(new Hideout(new Vector3(879, 8.5f, -355), 180));
        }
        internal static void Cleanup()
        {
            foreach (SteamPlayer player in Provider.clients)
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromSteamPlayer(player);
                if (player != null)
                {
                    freeHideout(uPlayer);
                }
            }
            freeHideouts.Clear();
            savedBarricades.Clear();
            claimedHideouts.Clear();
        }
        internal static Hideout getHideout(CSteamID playerId)
        {
            Hideout hideout = null;
            claimedHideouts.TryGetValue(playerId, out hideout);

            return hideout;
        }
        internal static void OnPlayerConnected(UnturnedPlayer player)
        {
            claimHideout(player);
        }
        internal static void OnPlayerDisconnect(UnturnedPlayer player)
        {
            freeHideout(player);
        }
        internal static void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            // hit != null barricade is placed on vehicle
            if (hit != null || !shouldAllow) return;

            CSteamID playerId = new CSteamID(owner);
           
            if (playerId == CSteamID.Nil || !claimedHideouts.TryGetValue(playerId, out Hideout hideout) || hideout == null) return;

            if (!hideout.isInBounds(point))
            {
                EffectControler.spawnUI(Conf.Notification_UI.UI_Id, Conf.Notification_UI.UI_Key, playerId);
                shouldAllow = false;
                Vector3 lower = hideout.bounds[0];
                Vector3 upper = hideout.bounds[1];

                Logger.Log($"placed at {point}, hideout bounds: lower x: {lower.x} y: {lower.y} z: {lower.z} upper x: {upper.x} y: {upper.y} z: {upper.z}");
                return;
            }
        }
        internal static void OnBarricadeSpawned(BarricadeRegion region, BarricadeDrop drop)
        {
            if (region is VehicleBarricadeRegion)
                return;

            addBarricade(drop);
        }
        internal static void OnBarricadeDestroy(BarricadeDrop drop, byte x, byte y, ushort plant)
        {
            removeBarricade(drop);
        }
        internal static void claimHideout(UnturnedPlayer player)
        {
            if (claimedHideouts.ContainsKey(player.CSteamID))
            {
                Logger.LogWarning($"{player.CSteamID} already has a hideout!");
                return;
            }
            if (freeHideouts.Count <= 0)
            {
                Logger.LogWarning("No more Hideouts available!");
                return;
            }
            Hideout claimedHideout = freeHideouts[0];
            claimedHideout.claim(player.CSteamID);
            claimedHideouts.Add(player.CSteamID, claimedHideout);
            freeHideouts.RemoveAt(0);

            restoreBarricades(player.CSteamID, claimedHideout);
        }
        internal static void freeHideout(UnturnedPlayer player)
        {
            if (!claimedHideouts.TryGetValue(player.CSteamID, out Hideout hideout) || hideout == null) 
                return;

            saveBarricades(player.CSteamID, hideout, true);
            hideout.free();
            claimedHideouts.Remove(player.CSteamID);
            freeHideouts.Add(hideout);
        }
        internal static void addBarricade(BarricadeDrop drop)
        {
            BarricadeData data = drop.GetServersideData();
            CSteamID playerId = new CSteamID(data.owner);
            if (!claimedHideouts.TryGetValue(playerId, out Hideout hideout))
            {
                Logger.LogWarning($"{playerId} has no hideout");
                return;
            }

            hideout.addBarricade(drop);
        }
        internal static void removeBarricade(BarricadeDrop drop)
        {
            BarricadeData data = drop.GetServersideData();
            if (data == null || data.owner == 0)
            {
                Logger.LogWarning("destroyed barricade without owner");
                return;
            }

            CSteamID ownerId = new CSteamID(data.owner);
            if (ownerId == CSteamID.Nil || !claimedHideouts.TryGetValue(ownerId, out Hideout hideout) || hideout == null) return;

            hideout.removeBarricade(drop);
        }
        internal static void saveBarricades(CSteamID playerId, Hideout hideout, bool clearHideout = false)
        {
            List<BarricadeWrapper> removedBarricades;
            if (clearHideout)
            {
                int barricadeCount = hideout.getBarricadeCount();
                if (!hideout.clearBarricades(out removedBarricades))
                {
                    Logger.LogError($"Could only clear {removedBarricades.Count}/{barricadeCount} Barricades of {playerId} hideout!");
                }
            }
            else
            {
                removedBarricades = hideout.getBarricades();
            }
            
            JsonManager.tryWriteToSaves(PlayerTool.getPlayer(playerId), SaveFileName, removedBarricades);
        }
        internal static void restoreBarricades(CSteamID playerId, Hideout hideout)
        {
            if(JsonManager.tryReadFromSaves(PlayerTool.getPlayer(playerId), SaveFileName, out List<BarricadeWrapper> barricades))
                hideout.restoreBarricades(barricades, playerId);
        }
    }
}
