using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Rocket.Unturned.Player;
using Logger = Rocket.Core.Logging.Logger;
using SpeedMann.Unturnov.Models.Hideout;


namespace SpeedMann.Unturnov.Helper
{
    public class EffectControler
    {
        private static ushort EventBorder_ID = 52200;
        private static float defaultBorderSize = 1;

        public static void spawnUI(ushort effectId, short effectKey, CSteamID executorID)
        {
            if (executorID == CSteamID.Nil)
                return;

            ITransportConnection transportConnection = Provider.findTransportConnection(executorID);
            if (transportConnection == null)
            {
                Logger.LogError($"Error trying to show UI (CSteamID not found {executorID})");
                return;
            }
            EffectManager.sendUIEffect(effectId, effectKey, transportConnection, true);
        }       

        internal static void hideBorders()
        {
            EffectManager.ClearEffectByID_AllPlayers(EventBorder_ID);
        }
        internal static void hideBorders(CSteamID steamID)
        {
            ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
            if (transportConnection == null)
            {
                Logger.LogError($"Error trying to hide EventBorder (CSteamID not found {steamID})");
                return;
            }

            EffectManager.askEffectClearByID(EventBorder_ID, transportConnection);
        }
        internal static void spawnBorders(CSteamID executorID, Hideout hideout)
        {
            if (hideout == null)
            {
                return;
            }
                
            Vector3[] points = new Vector3[4]
            {
                hideout.bounds[0],
                new Vector3(hideout.bounds[0].x, hideout.bounds[0].y, hideout.bounds[1].z),
                hideout.bounds[1],
                new Vector3(hideout.bounds[1].x, hideout.bounds[0].y, hideout.bounds[0].z),
            };
            spawnBorder(executorID, points[0], points[1], hideout.bounds[0].y, hideout.bounds[1].y);
            spawnBorder(executorID, points[1], points[2], hideout.bounds[0].y, hideout.bounds[1].y);
            spawnBorder(executorID, points[2], points[3], hideout.bounds[0].y, hideout.bounds[1].y);
            spawnBorder(executorID, points[3], points[0], hideout.bounds[0].y, hideout.bounds[1].y);
        }
        internal static void spawnBorder(CSteamID executorID, Vector3 pointA, Vector3 pointB, float lowestPoint, float heighestPoint)
        {
            calcBorderValues(pointA, pointB, lowestPoint, heighestPoint, out Vector3 position, out Vector3 rotation, out Vector3 scale);
            spawnBorder(executorID, position, rotation, scale);
        }
        internal static void spawnBorder(Vector3 pointA, Vector3 pointB, float lowestPoint, float heighestPoint)
        {
            calcBorderValues(pointA, pointB, lowestPoint, heighestPoint, out Vector3 position, out Vector3 rotation, out Vector3 scale);
            spawnBorder(position, rotation, scale);
        }
        
        internal static void spawnBorder(Vector3 point, Vector3 rotate, Vector3 scale)
        {
            foreach (SteamPlayer player in Provider.clients)
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromSteamPlayer(player);
                if (player != null)
                {
                    spawnBorder(uPlayer.CSteamID, point, rotate, scale);
                }
            }
        }
        internal static void spawnBorder(CSteamID executorID, Vector3 point, Vector3 rotate, Vector3 scale)
        {
            ITransportConnection transportConnection = Provider.findTransportConnection(executorID);
            if (transportConnection == null)
            {
                Logger.LogError($"Error trying to show EventBorder (CSteamID not found {executorID})");
                return;
            }

            EffectAsset effectAsset = Assets.find(EAssetType.EFFECT, EventBorder_ID) as EffectAsset;

            if(effectAsset == null)
            {
                Logger.LogError($"Error trying to show EventBorder (effect asset not found {EventBorder_ID})");
                return;
            }

            TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
            parameters.reliable = true;
            parameters.SetRelevantPlayer(transportConnection);
            parameters.position = point;
            parameters.direction = rotate;
            parameters.scale = scale;

            EffectManager.triggerEffect(parameters);

            Logger.Log($"Spawned border at: {point}");
        }

        #region Helper Functions
        private static void calcBorderValues(Vector3 pointA, Vector3 pointB, float lowestPoint, float heighestPoint, out Vector3 position, out Vector3 rotation, out Vector3 scale)
        {
            float height = (heighestPoint - lowestPoint) / defaultBorderSize;

            Vector3 pos1 = new Vector3(pointA.x, lowestPoint, pointA.z);
            Vector3 pos2 = new Vector3(pointB.x, lowestPoint, pointB.z);

            position = (pos1 + pos2) / 2f;
            rotation = pos2 - pos1;
            float requiredSizeZ = rotation.magnitude;
            scale = new Vector3(1, height, requiredSizeZ / defaultBorderSize);
        }
        #endregion
    }
}
