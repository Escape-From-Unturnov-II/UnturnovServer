using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using SpeedMann.Unturnov.Models.Hideout;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Controlers
{
    internal class HideoutControler
    {
        // TODO: move restore barricade coroutine from hideout to hideout controler
        // handle a list of hideout barricade lists to better define barricades placed per frame 
        // let hideout remove its own barricade list on free
        // keep an index of the placed ones to save partialy created hideouts
        public delegate void HideoutClearUpdate(CSteamID ownerId, bool clearing);
        public static event HideoutClearUpdate OnHideoutClearUpdate;

        private static HideoutConfig Conf;
        private static Dictionary<CSteamID, Hideout> claimedHideouts;
        private static List<Hideout> freeHideouts;
        private static List<GameObject> hideoutObjects;
        private static string SaveFileName = "Hideout";

        internal static void Init(HideoutConfig hideoutConfig)
        {
            Conf = hideoutConfig;
            claimedHideouts = new Dictionary<CSteamID, Hideout>();
            freeHideouts = new List<Hideout>();
            hideoutObjects = new List<GameObject>();
            foreach (var position in Conf.HideoutPositions)
            {
                createHideout(position.GetVector3(), position.rot);
            }
            createHideout(new Vector3(868, 8.5f, -350), 0);
            createHideout(new Vector3(879, 8.5f, -350), 0);
            createHideout(new Vector3(879, 8.5f, -355), 180);

            //TODO: add checks for any barricade interactions and prevent them if hideout is not ready
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
            claimedHideouts.Clear();
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
            if (hit != null || !shouldAllow) 
                return;

            CSteamID playerId = new CSteamID(owner);
           
            if (playerId == CSteamID.Nil || !claimedHideouts.TryGetValue(playerId, out Hideout hideout) || hideout == null)
            {
                shouldAllow = false;
                Logger.Log($"Hideout of {playerId} not found!");
                return;
            }
                

            if (!hideout.isReady())
            {
                shouldAllow = false;
                Logger.Log($"Hideout of {playerId} not ready!");
                return;
            }
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
        internal static void OnBarricadeSalvageRequest(BarricadeDrop barricadeDrop, SteamPlayer instigatorClient, ref bool shouldAllow)
        {
            if (!shouldAllow) 
                return;

            if (!isHideoutReady(new CSteamID(barricadeDrop.GetServersideData().owner)))
            {
                shouldAllow = false;

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
        internal static void OnGetInput(InputInfo inputInfo, ERaycastInfoUsage usage, ref bool shouldAllow)
        {
            if (!shouldAllow)
                return;

            if (inputInfo.type != ERaycastInfoType.BARRICADE || !BarricadeHelper.tryGetBarricadeDrop(inputInfo.transform, out var drop))
                return;

            if (!isHideoutReady(new CSteamID(drop.GetServersideData().owner)))
                shouldAllow = false;

        }
        internal static void OnBarricadeStorageRequest(InteractableStorage storage, ServerInvocationContext context, bool quickGrab, ref bool shouldAllow)
        {
            if (!shouldAllow)
                return;

            if (!isHideoutReady(storage.owner))
                shouldAllow = false;
        }
        internal static void createHideout(Vector3 origin, float rotation)
        {
            GameObject hideoutObject = new GameObject();
            Hideout hideout = hideoutObject.AddComponent<Hideout>();
            hideout.Initialize(origin, rotation);
            hideoutObjects.Add(hideoutObject);

            freeHideouts.Add(hideout);
        }
        internal static Hideout getHideout(CSteamID playerId)
        {
            if (!claimedHideouts.TryGetValue(playerId, out var hideoutObject))
                return null;

            return hideoutObject.GetComponent<Hideout>();
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
            Hideout hideout = freeHideouts[0];
            if (hideout == null)
            {
                Logger.LogError("cant claim GameObject it has no hideout component");
                return;
            }
            JsonManager.tryReadFromSaves(PlayerTool.getPlayer(player.CSteamID), SaveFileName, out List<BarricadeWrapper> barricades);

            hideout.claim(player.CSteamID, barricades);
            claimedHideouts.Add(player.CSteamID, hideout);
            freeHideouts.RemoveAt(0);
        }
        internal static bool isHideoutReady(CSteamID playerId)
        {
            if (claimedHideouts.TryGetValue(playerId, out Hideout hideout))
            {
                return hideout.isReady();
            }
            return false;
        }
        internal static void freeHideout(UnturnedPlayer player)
        {
            if (!claimedHideouts.TryGetValue(player.CSteamID, out var claimedHideout)) 
                return;

            if (claimedHideout == null)
                return;
            claimedHideout.freeWhenReady(freeInner);
        }
        internal static void addBarricade(BarricadeDrop drop)
        {
            BarricadeData data = drop.GetServersideData();
            CSteamID playerId = new CSteamID(data.owner);
            if (!claimedHideouts.TryGetValue(playerId, out var hideoutObject))
            {
                Logger.LogWarning($"{playerId} has no hideout");
                return;
            }

            Hideout hideout = hideoutObject.GetComponent<Hideout>();
            if (hideout == null)
                return;

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
            if (ownerId == CSteamID.Nil || !claimedHideouts.TryGetValue(ownerId, out var hideoutObject)) 
                return;

            Hideout hideout = hideoutObject.GetComponent<Hideout>();
            if (hideout == null)
                return;

            hideout.removeBarricade(drop);
        }
        internal static void saveAndClearBarricades(CSteamID playerId, Hideout hideout, bool clearHideout = false)
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
        private static void freeInner(Hideout hideout)
        {
            OnHideoutClearUpdate?.Invoke(hideout.owner, true);
            saveAndClearBarricades(hideout.owner, hideout, true);
            claimedHideouts.Remove(hideout.owner);
            freeHideouts.Add(hideout);
            OnHideoutClearUpdate?.Invoke(hideout.owner, false);
        }
    }
}
