using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
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
        public static void Init(UnturnovConfiguration config)
        {
            Conf = config.PlacementRestrictionConfig;
            createDictionaryForPlacementRestrictions(Conf.Restrictions, Conf.FoundationSets);
            PlacementRestrictionDict = Unturnov.createDictionaryFromItemExtensions(Conf.Restrictions);
        }
        internal static void OnBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            // hit != null barricade is placed on vehicle

            if (PlacementRestrictionDict.TryGetValue(barricade.asset.id, out PlacementRestriction restriction))
            {
                shouldAllow = false;

                Regions.tryGetCoordinate(point, out byte x, out byte y);
                List<RegionCoordinate> coordinates = new List<RegionCoordinate>() { new RegionCoordinate(x, y) };
                List<Transform> transformsObjects = new List<Transform>();
                List<Transform> transformsBarricades = new List<Transform>();

                // TODO: replace get_InRadius with raicast
                // find object
                ObjectManager.getObjectsInRadius(new Vector3(point.x, point.y + Conf.SearchCenterHeightChange, point.z), Conf.SearchRadius, coordinates, transformsObjects);
                // find barricade
                BarricadeManager.getBarricadesInRadius(new Vector3(point.x, point.y + Conf.SearchCenterHeightChange, point.z), Conf.SearchRadius, coordinates, transformsBarricades);
                if (Conf.Debug)
                {
                    Logger.Log($"Barricade was placed near " +
                        $"{(transformsBarricades.Count > 0 ? "barricades: " + string.Join(", ", transformsBarricades.Select(t => $"{t.name}").ToArray()) + " " : "")}" +
                        $"{ (transformsObjects.Count > 0 ? "objects: " + string.Join(", ", transformsObjects.Select(t => $"{t.name}").ToArray()) : "")}");
                }
                foreach (Transform transform in transformsObjects)
                {
                    if(int.TryParse(transform.name, out int id))
                    {
                        if (restriction.ValidObjectFoundations.ContainsKey((ushort)id))
                        {
                            shouldAllow = true;
                            return;
                        }
                    }
                }
                foreach (Transform transform in transformsBarricades)
                {
                    if (int.TryParse(transform.name, out int id))
                    {
                        if (restriction.ValidItemFoundations.ContainsKey((ushort)id))
                        {
                            shouldAllow = true;
                            return;
                        }
                    }
                }
            }
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
                                    selectedDict = restriction.ValidItemFoundations;
                                    break;
                                case EAssetType.OBJECT:
                                    selectedDict = restriction.ValidObjectFoundations;
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
