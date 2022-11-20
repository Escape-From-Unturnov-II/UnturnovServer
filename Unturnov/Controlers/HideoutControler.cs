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
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Controlers
{
    internal class HideoutControler
    {
        private static HideoutConfig Conf;
        private static Dictionary<CSteamID, List<BarricadeWrapper>> hideoutBarricades = new Dictionary<CSteamID, List<BarricadeWrapper>>();
        private static Dictionary<CSteamID, List<BarricadeWrapper>> savedBarricades = new Dictionary<CSteamID, List<BarricadeWrapper>>();
        private static Dictionary<CSteamID, Hideout> claimedHideouts = new Dictionary<CSteamID, Hideout>();
        private static List<Hideout> freeHideouts = new List<Hideout>();

        internal static void Init(HideoutConfig hideoutConfig)
        {
            Conf = hideoutConfig;
            claimedHideouts = new Dictionary<CSteamID, Hideout>();

            Vector3 centerA = new Vector3(868, 8.5f, -350);
            Vector3 centerB = new Vector3(879, 8.5f, -350);

            //TODO fix rotation
            freeHideouts.Add(new Hideout(centerA, 0));
            freeHideouts.Add(new Hideout(centerB, 0));
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

            addBarricade(playerId, asset, point, new Vector3(angle_x, angle_y, angle_z));
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
            claimedHideouts.Add(player.CSteamID, claimedHideout);
            freeHideouts.RemoveAt(0);

            if (hideoutBarricades.ContainsKey(player.CSteamID))
            {
                Logger.LogWarning($"{player.CSteamID} already has hideout barricades!");
                return;
            }
            hideoutBarricades.Add(player.CSteamID, new List<BarricadeWrapper>());

            restoreBarricades(player.CSteamID, claimedHideout);
        }

        internal static void freeHideout(UnturnedPlayer player)
        {
            if (!claimedHideouts.TryGetValue(player.CSteamID, out Hideout hideout) || hideout == null) return;

            saveBarricades(player.CSteamID, hideout);

            claimedHideouts.Remove(player.CSteamID);
            freeHideouts.Add(hideout);

            hideoutBarricades.Remove(player.CSteamID);
        }

        internal static void addBarricade(CSteamID playerId, ItemBarricadeAsset barricade, Vector3 location, Vector3 rotation)
        {
            if (!claimedHideouts.ContainsKey(playerId))
            {
                Logger.LogWarning($"{playerId} has no hideout");
                return;
            }
            if(!hideoutBarricades.ContainsKey(playerId))
            {
                Logger.LogWarning($"{playerId} has no hideout");
                return;
            }

            hideoutBarricades[playerId].Add(new BarricadeWrapper(barricade.id, location, rotation));
        }
        internal static void saveBarricades(CSteamID playerId, Hideout hideout)
        {
            if (!hideoutBarricades.ContainsKey(playerId))
            {
                Logger.Log($"{playerId} has no barricades");
                return;
            }
            if (!savedBarricades.ContainsKey(playerId))
            {
                savedBarricades.Add(playerId, new List<BarricadeWrapper>());
            }
            savedBarricades[playerId].Clear();

            foreach (BarricadeWrapper barricade in hideoutBarricades[playerId])
            {
                BarricadeHelper.tryDestroyBarricade(barricade.location, barricade.id);
                savedBarricades[playerId].Add(hideout.convertToRelativePosition(barricade));
            }
            hideoutBarricades.Clear();
        }
        internal static void restoreBarricades(CSteamID playerId, Hideout hideout)
        {
            if (!savedBarricades.ContainsKey(playerId))
            {
                Logger.Log($"{playerId} has no barricades");
                return;
            }
            if (!hideoutBarricades.ContainsKey(playerId))
            {
                hideoutBarricades.Add(playerId, new List<BarricadeWrapper>());
            }
            hideoutBarricades[playerId].Clear();

            foreach (BarricadeWrapper barricade in savedBarricades[playerId])
            {
                hideout.restoreBarricade(barricade, playerId);
            }

            savedBarricades.Clear();
        }
    }
}
