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
        private static Dictionary<CSteamID, Hideout> claimedHideouts = new Dictionary<CSteamID, Hideout>();
        private static List<Hideout> freeHideouts = new List<Hideout>();

        internal static void Init(HideoutConfig hideoutConfig)
        {
            Conf = hideoutConfig;

            Vector3 centerA = new Vector3(867, 8.5f, -355);
            Vector3 centerB = new Vector3(857, 8.5f, -350);
            
            freeHideouts.Add(new Hideout(centerA, 0, getBounds(centerA, 0)));
            freeHideouts.Add(new Hideout(centerB, 90, getBounds(centerB, 90)));

        }

        private static Vector3[] getBounds(Vector3 point, float rotation)
        {
            float height = 5;
            float length = 10;
            float width = 8;
            Vector3 offset = new Vector3(); 

            Vector3[] bounds = new Vector3[2];

            bounds[0] = point + Quaternion.Euler(rotation, 0, 0) * offset;
            bounds[1] = new Vector3(point.x + length, point.y + height, point.z + width) + Quaternion.Euler(rotation, 0, 0) * offset;

            Logger.Log($"upper bound x: {bounds[1].x} y: {bounds[1].y} z: {bounds[1].z}");

            return bounds;
        }
        internal static void OnPlayerConnected(UnturnedPlayer player)
        {
            if(freeHideouts.Count <= 0)
            {
                Logger.LogWarning("No more Hideouts available!");
                return; 
            }
            if (claimedHideouts.ContainsKey(player.CSteamID))
            {
                Logger.LogWarning($"{player.CSteamID} already has a hideout!");
                return;
            }
            claimedHideouts.Add(player.CSteamID, freeHideouts[0]);
            freeHideouts.RemoveAt(0);
        }
        internal static void OnPlayerDisconnect(UnturnedPlayer player)
        {          
            if (!claimedHideouts.TryGetValue(player.CSteamID, out Hideout hideout) || hideout == null) return;

            claimedHideouts.Remove(player.CSteamID);
            freeHideouts.Add(hideout);
        }
        internal static void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            // hit != null barricade is placed on vehicle
            if (!shouldAllow) return;

            if (!PlacementRestrictionControler.tryFindPlacingPlayer(asset.id, out CSteamID playerId) || playerId == CSteamID.Nil)
            {
                return;
            }
            if (!claimedHideouts.TryGetValue(playerId, out Hideout hideout) || hideout == null) return;

            if (!hideout.isInBounds(point))
            {
                EffectControler.spawnUI(Conf.Notification_UI.UI_Id, Conf.Notification_UI.UI_Key, playerId);
                shouldAllow = false;
            }
        }
    }
}
