using Org.BouncyCastle.Asn1.IsisMtt.X509;
using Rocket.Core.Assets;
using Rocket.Unturned.Player;
using SDG.Framework.Utilities;
using SDG.Unturned;
using SpeedMann.Unturnov.Controlers;
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

namespace SpeedMann.Unturnov.Helper
{
    public class PlacementRestrictionControler
    {
        private static PlacementRestrictionConfig Conf;
        private static Dictionary<ushort, PlacementRestriction> PlacementRestrictionDict;
        private static Dictionary<CSteamID, BarricadePlacementInfo> PlayerPlacementInfoDict;
        private static List<ulong> BypassBarricadeConnectionList;
        
        public static void Init(PlacementRestrictionConfig config)
        {
            Conf = config;
            createDictionaryForPlacementRestrictions(Conf.Restrictions, Conf.FoundationSets);
            PlacementRestrictionDict = Unturnov.createDictionaryFromItemExtensions(Conf.Restrictions);
            PlayerPlacementInfoDict = new Dictionary<CSteamID, BarricadePlacementInfo>();
            BypassBarricadeConnectionList = new List<ulong>();
            HideoutControler.OnHideoutClearUpdate += UpdateBarricadeConnectionBypassList;
        }

        public static void UpdateBarricadeConnectionBypassList(CSteamID playerId, bool bypass)
        {
            if (bypass && !BypassBarricadeConnectionList.Contains(playerId.m_SteamID))
            {
                BypassBarricadeConnectionList.Add(playerId.m_SteamID);
                return;
            }

            BypassBarricadeConnectionList.Remove(playerId.m_SteamID);
        }
        public static void Cleanup()
        {
            PlacementRestrictionDict.Clear();
            PlayerPlacementInfoDict.Clear();
            BarricadeConnections.clear();
        }
        
        internal static void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!shouldAllow || !PlacementRestrictionDict.TryGetValue(asset.id, out PlacementRestriction restriction) || restriction == null) 
                return;

            if (!tryGetObjectOrBarricadeBelow(point, restriction, asset, owner, out BarricadeDrop barricadeFoundation, out ObjectAsset objectFoundation))
            {
                shouldAllow = false;
                EffectControler.spawnUI(Conf.Notification_UI.UI_Id, Conf.Notification_UI.UI_Key, new CSteamID(owner));
                return;
            }

            string target = "unknown";
            if (barricadeFoundation != null)
            {
                addPlacementInfo(new BarricadePlacementInfo(barricadeFoundation, asset), owner);
                target = barricadeFoundation.asset.name;
            }
            else if (objectFoundation != null)
            {
                target = objectFoundation.name;
            }

            if (Conf.Debug)
            {
                Logger.Log($"RestrictedBarricade was placed on {target}");
            }
        }
        internal static void OnBarricadeSpawned(BarricadeRegion region, BarricadeDrop drop)
        {
            CSteamID playerId = new CSteamID(drop.GetServersideData().owner);
            var data = drop.GetServersideData();
            if(Conf.Debug)
                Logger.Log($"Player {playerId} placed barricade {drop.asset.id} position {data.point} rotation {new Vector3(data.angle_x, data.angle_y, data.angle_z)}");

            if(!PlayerPlacementInfoDict.TryGetValue(playerId, out var placementInfo))
            {
                // try find foundation below spawned barricades
                if (PlacementRestrictionDict.TryGetValue(drop.asset.id, out PlacementRestriction restriction) && tryGetObjectOrBarricadeBelow(data.point, restriction, drop.asset, playerId.m_SteamID, out BarricadeDrop barricadeFoundation, out ObjectAsset objectFoundation) && barricadeFoundation != null)
                {
                    Logger.Log("added new PlacementInfo");
                    placementInfo = new BarricadePlacementInfo(barricadeFoundation, drop.asset);
                }
                else
                {
                    return;
                }
            }

            BarricadeConnections.tryAddBarricadeConnection(placementInfo.foundation, drop);
           
            PlayerPlacementInfoDict.Remove(playerId);
        }
        internal static void OnBarricadeDestroy(BarricadeDrop drop, byte x, byte y, ushort plant)
        {
            if(Conf.Debug)
                Logger.Log($"Destoyed barricade {drop.asset.id} at {drop.model.position}");
            if (!PlacementRestrictionDict.ContainsKey(drop.asset.id) && !BypassBarricadeConnectionList.Contains(drop.GetServersideData().owner))
            {
                List<BarricadeDrop> connectedDrops = BarricadeConnections.getConnectedBarricades(drop);

                if (Conf.Debug)
                    Logger.Log($"Removed foundation {drop.asset.id}, with {connectedDrops.Count} connected barricades");

                while (connectedDrops.Count > 0)
                {
                    if (!BarricadeHelper.trySalvageBarricade(connectedDrops[0]))
                    {
                        break;
                    }
                }
            }
            BarricadeConnections.removeBarricade(drop);
        }
        #region Helper Functions

        private static void createDictionaryForPlacementRestrictions(List<PlacementRestriction> placementRestrictions, List<FoundationSet> foundationSets)
        {
            foreach (PlacementRestriction restriction in placementRestrictions)
            {
                foreach (string name in restriction.ValidFoundationSetNames)
                {
                    if (tryGetFoundationSetByName(name, foundationSets, out List<PlacementFoundation> foundationSet))
                    {
                        foreach (PlacementFoundation foundation in foundationSet)
                        {
                            Dictionary<ushort, PlacementFoundation> selectedDict;
                            switch (foundation.type)
                            {
                                case EAssetType.ITEM:
                                    selectedDict = restriction.ValidBarricades;
                                    break;
                                case EAssetType.OBJECT:
                                    selectedDict = restriction.ValidObjects;
                                    break;
                                default:
                                    Logger.LogError($"Foundation with Id: {foundation.Id} has invalid type {foundation.type}! \n" +
                                        $"Valid types are ITEM and OBJECT");
                                    continue;
                            }
                            if (selectedDict.ContainsKey(foundation.Id))
                            {
                                Logger.LogWarning($"Foundation with Id: {foundation.Id} and type: {foundation.type} is a duplicate!");
                            }
                            else
                            {
                                selectedDict.Add(foundation.Id, foundation);
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning("FoundationSet with name:" + name + " was not found!");
                    }
                }
            }
        }
        private static bool tryGetFoundationSetByName(string name, List<FoundationSet> foundationSets, out List<PlacementFoundation> set)
        {
            set = new List<PlacementFoundation>();
            foreach (FoundationSet list in foundationSets)
            {
                if (list.Name.ToLower().Equals(name.ToLower()))
                {
                    set = list.Foundations;
                    return true;
                }
            }

            return false;
        }
        private static bool tryGetObjectOrBarricadeBelow(Vector3 origin, PlacementRestriction restriction, ItemBarricadeAsset asset, ulong owner, out BarricadeDrop barricadeFoundation, out ObjectAsset objectFoundation)
        {
            objectFoundation = null;
            barricadeFoundation = null;

            Physics.Raycast(new Vector3(origin.x, origin.y + Conf.Offset, origin.z), Vector3.down, out RaycastHit raycastHit, Conf.Distance, RayMasks.BLOCK_COLLISION);
            if (raycastHit.transform == null) 
                return false;
            switch (raycastHit.transform.tag)
            {
                case "Barricade":
                    BarricadeDrop barricadeDrop = BarricadeManager.FindBarricadeByRootTransform(raycastHit.transform);
                    if (barricadeDrop?.asset != null && restriction.ValidBarricades.ContainsKey(barricadeDrop.asset.id))
                    {
                        barricadeFoundation = barricadeDrop;
                        return true;
                    }
                    break;
                case "Large":
                case "Medium":
                case "Small":
                    ObjectAsset objectAsset = LevelObjects.getAsset(raycastHit.transform);
                    if (objectAsset != null && restriction.ValidObjects.ContainsKey(objectAsset.id))
                    {
                        objectFoundation = objectAsset;
                        return true;
                    }
                    break;
            }
            return false;
        }
        private static void addPlacementInfo(BarricadePlacementInfo info, ulong ownerId)
        {
            if (info == null)
                return;

            CSteamID steamId = new CSteamID(ownerId);

            if (PlayerPlacementInfoDict.ContainsKey(steamId))
            {
                PlayerPlacementInfoDict[steamId] = info;
                return;
            }
            PlayerPlacementInfoDict.Add(steamId, info);
        }
        internal class BarricadePlacementInfo
        {
            internal BarricadeDrop foundation;
            internal ItemBarricadeAsset placedBarricade;
            internal BarricadePlacementInfo(BarricadeDrop foundation, ItemBarricadeAsset placedBarricade)
            {
                this.foundation = foundation;
                this.placedBarricade = placedBarricade;
            }
        }
        internal class BarricadeConnections
        {
            private static Dictionary<BarricadeDrop, List<BarricadeDrop>> ConnectedBarricades = new Dictionary<BarricadeDrop, List<BarricadeDrop>>();
            private static Dictionary<BarricadeDrop, BarricadeDrop> ReverseConnection = new Dictionary<BarricadeDrop, BarricadeDrop>();
            internal static bool tryAddBarricadeConnection(BarricadeDrop foundation, BarricadeDrop addedBarricade)
            {
                if (foundation == null || addedBarricade == null)
                {
                    Logger.LogError("Could not add barricade connection for null barricade or null foundation");
                    return false;
                }

                if (!tryAddReverseConnection(foundation, addedBarricade))
                    return false;

                if (Conf.Debug)
                    Logger.Log($"Added connection from barricade {addedBarricade.asset.id}, to foundation {foundation.asset.id}");

                if (ConnectedBarricades.TryGetValue(foundation, out List<BarricadeDrop> connectedDrops))
                {
                    connectedDrops.Add(addedBarricade);
                    return true;
                }
                ConnectedBarricades.Add(foundation, new List<BarricadeDrop> { addedBarricade });
                return true;
            }
            internal static void removeBarricade(BarricadeDrop barricade)
            {
                if (barricade == null)
                    return;

                if (ConnectedBarricades.TryGetValue(barricade, out List<BarricadeDrop> connectedBarricades))
                {
                    removeBarricadeFoundation(barricade, connectedBarricades);
                    return;
                }
                if (ReverseConnection.TryGetValue(barricade, out BarricadeDrop foundation) && foundation != null)
                {
                    removeRestrictedBarricade(barricade, foundation);
                }
            }
            internal static void clear()
            {
                ConnectedBarricades.Clear();
                ReverseConnection.Clear();
            }
            internal static List<BarricadeDrop> getConnectedBarricades(BarricadeDrop foundation)
            {
                if (ConnectedBarricades.TryGetValue(foundation, out List<BarricadeDrop> connectedDrops))
                {
                    return connectedDrops;
                }
                return new List<BarricadeDrop>();
            }
            private static void removeBarricadeFoundation(BarricadeDrop barricade, List<BarricadeDrop> connectedBarricades)
            {
                foreach (BarricadeDrop connectedBarricade in connectedBarricades)
                {
                    ReverseConnection.Remove(connectedBarricade);
                }

                ConnectedBarricades.Remove(barricade);
            }
            private static void removeRestrictedBarricade(BarricadeDrop barricade, BarricadeDrop foundation)
            {
                if (!ConnectedBarricades.TryGetValue(foundation, out List<BarricadeDrop> connectedBarricades))
                    return;

                connectedBarricades.Remove(barricade);
            }
            private static bool tryAddReverseConnection(BarricadeDrop foundation, BarricadeDrop addedBarricade)
            {
                if (ReverseConnection.TryGetValue(addedBarricade, out BarricadeDrop currentFoundation))
                {
                    Logger.LogError($"Barricade {addedBarricade.asset.id} is already connected to a foundation {currentFoundation.asset.id}");
                    return false;
                }
                ReverseConnection.Add(addedBarricade, foundation);
                return true;
            }
        }
        
        #endregion
    }
}
