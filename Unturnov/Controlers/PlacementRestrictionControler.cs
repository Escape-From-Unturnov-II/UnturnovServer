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

        private static Dictionary<CSteamID, ushort> lastPlaceRequest = new Dictionary<CSteamID, ushort>();

        public static void Init(PlacementRestrictionConfig config)
        {
            Conf = config;
            createDictionaryForPlacementRestrictions(Conf.Restrictions, Conf.FoundationSets);
            PlacementRestrictionDict = Unturnov.createDictionaryFromItemExtensions(Conf.Restrictions);
        }
        internal static void OnPlayerDisconnect(UnturnedPlayer player)
        {
            lastPlaceRequest.Remove(player.CSteamID);
        }
        internal static void OnUseBarricade(UseableBarricade useableBarricade, bool post)
        {
            if (useableBarricade?.player?.equipment?.asset == null) return;
            ItemAsset asset = useableBarricade.player.equipment.asset;

            UnturnedPlayer player = UnturnedPlayer.FromPlayer(useableBarricade.player);

            if (!post)
            {
                if (lastPlaceRequest.ContainsKey(player.CSteamID))
                {
                    lastPlaceRequest[player.CSteamID] = asset.id;
                    return;
                }

                lastPlaceRequest.Add(player.CSteamID, asset.id);
                return;
            }

            lastPlaceRequest.Remove(player.CSteamID);
        }
        internal static void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            // hit != null barricade is placed on vehicle

            if (!shouldAllow) return;

            if (PlacementRestrictionDict.TryGetValue(asset.id, out PlacementRestriction restriction) && restriction != null)
            {
                shouldAllow = false;

                Physics.Raycast(new Vector3(point.x, point.y + Conf.Offset, point.z), Vector3.down, out RaycastHit raycastHit, Conf.Offset * 2, RayMasks.BLOCK_COLLISION);
                if (raycastHit.transform == null) return;

                string target = "unknown";
                switch(raycastHit.transform.tag)
                {
                    case "Barricade":
                        BarricadeDrop barricadeDrop = BarricadeManager.FindBarricadeByRootTransform(raycastHit.transform);
                        if (barricadeDrop?.asset != null && restriction.ValidBarricades.ContainsKey(barricadeDrop.asset.id))
                        {
                            shouldAllow = true;
                            target = barricadeDrop.asset.name;
                        }
                        break;
                    case "Large":
                    case "Medium":
                    case "Small":
                        ObjectAsset objectAsset = LevelObjects.getAsset(raycastHit.transform);
                        if (objectAsset != null && restriction.ValidObjects.ContainsKey(objectAsset.id))
                        {
                            shouldAllow = true;
                            target = objectAsset.name;
                        }
                        break;
                }
                if (shouldAllow && Conf.Debug)
                {
                    Logger.Log($"RestrictedBarricade was placed on {target}");
                }

                if (!shouldAllow && tryFindPlacingPlayer(asset.id, out CSteamID playerId))
                {
                    EffectControler.spawnUI(Conf.Notification_UI.UI_Id, Conf.Notification_UI.UI_Key, playerId);
                }
            }

        }
        internal static bool tryFindPlacingPlayer(ushort itemId, out CSteamID playerId)
        {
            playerId = CSteamID.Nil;
            foreach (KeyValuePair<CSteamID, ushort> entry in lastPlaceRequest)
            {
                if(entry.Value == itemId)
                {
                    playerId = entry.Key;
                    return true;
                }
            }
            return false;
        }
        internal static void createDictionaryForPlacementRestrictions(List<PlacementRestriction> placementRestrictions, List<FoundationSet> foundationSets)
        {
            foreach (PlacementRestriction restriction in placementRestrictions)
            {
                foreach (string name in restriction.ValidFoundationSetNames)
                {
                    if (tryGetFoundationSet(name, foundationSets, out List<PlacementFoundation> foundationSet))
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
        internal static bool tryGetFoundationSet(string name, List<FoundationSet> foundationSets, out List<PlacementFoundation> set)
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
    }
}
