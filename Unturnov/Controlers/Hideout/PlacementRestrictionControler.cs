using Org.BouncyCastle.Asn1.IsisMtt.X509;
using Rocket.Core.Assets;
using Rocket.Unturned.Player;
using SDG.Framework.Utilities;
using SDG.Unturned;
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
        
        public static void Init(PlacementRestrictionConfig config)
        {
            Conf = config;
            createDictionaryForPlacementRestrictions(Conf.Restrictions, Conf.FoundationSets);
            PlacementRestrictionDict = Unturnov.createDictionaryFromItemExtensions(Conf.Restrictions);
            PlayerPlacementInfoDict = new Dictionary<CSteamID, BarricadePlacementInfo>();
        }

        public static void Cleanup()
        {
            PlacementRestrictionDict.Clear();
            PlayerPlacementInfoDict.Clear();
            BarricadeConnections.clear();
        }
        
        internal static void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            
            if (!shouldAllow || !PlacementRestrictionDict.TryGetValue(asset.id, out PlacementRestriction restriction) || restriction != null) 
                return;

            if (!tryGetFoundation(point, restriction, asset, owner, out BarricadeDrop barricadeFoundation, out ObjectAsset objectFoundation))
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

            if(!PlayerPlacementInfoDict.TryGetValue(playerId, out var placementInfo))
            {
                return;
            }

            BarricadeConnections.tryAddBarricadeConnection(placementInfo.foundation, drop);
           
            PlayerPlacementInfoDict.Remove(playerId);
        }
        internal static void OnBarricadeDestroy(BarricadeDrop drop, byte x, byte y, ushort plant)
        {
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
        private static bool tryGetFoundation(Vector3 origin, PlacementRestriction restriction, ItemBarricadeAsset asset, ulong owner, out BarricadeDrop barricadeFoundation, out ObjectAsset objectFoundation)
        {
            objectFoundation = null;
            barricadeFoundation = null;
            Physics.Raycast(new Vector3(origin.x, origin.y + Conf.Offset, origin.z), Vector3.down, out RaycastHit raycastHit, Conf.Offset * 2, RayMasks.BLOCK_COLLISION);
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
            private static Dictionary<BarricadeDrop, List<BarricadeDrop>> ConnectedBarricades;
            private static Dictionary<BarricadeDrop, BarricadeDrop> ReverseConnection;
            internal static bool tryAddBarricadeConnection(BarricadeDrop foundation, BarricadeDrop addedBarricade)
            {
                if (foundation == null || addedBarricade == null)
                {
                    if (Conf.Debug)
                        Logger.LogWarning("Could not add barricade connection for null barricade or null foundation");
                    return false;
                }

                if (!tryAddReverseConnection(foundation, addedBarricade))
                    return false;

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
                if (ReverseConnection.TryGetValue(barricade, out BarricadeDrop foundation) || foundation == null)
                {
                    removeRestrictedBarricade(barricade, foundation);
                }
            }
            internal static void clear()
            {
                ConnectedBarricades.Clear();
                ReverseConnection.Clear();
            }
            private static void removeBarricadeFoundation(BarricadeDrop barricade, List<BarricadeDrop> connectedBarricades)
            {
                foreach (BarricadeDrop connectedBarricade in connectedBarricades)
                {
                    ReverseConnection.Remove(connectedBarricade);
                    UnturnedPrivateFields.TrySendSalvageRequest(connectedBarricade);
                    //TODO check what happens on salvage (do items drop?)
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
