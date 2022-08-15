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

            if (PlacementRestrictionDict.TryGetValue(asset.id, out PlacementRestriction restriction))
            {
                shouldAllow = false;

                PhysicsEx.Raycast(new Vector3(point.x, point.y + 0.5f, angle_z), Vector3.down, out RaycastHit rayHit, 1, RayMasks.SLOTS_INTERACT, QueryTriggerInteraction.UseGlobal);

                if (rayHit.transform == null) return;

                switch(rayHit.transform.tag)
                {
                    case "Barricade":
                        BarricadeDrop barricadeDrop = BarricadeManager.FindBarricadeByRootTransform(rayHit.transform);
                        if (barricadeDrop?.asset != null && restriction.ValidBarricades.ContainsKey(barricadeDrop.asset.id))
                        {
                            shouldAllow = true;
                            if (Conf.Debug)
                            {
                                Logger.Log($"RestrictedBarricade was placed on {barricadeDrop.asset.name}");
                            }
                        }
                        break;
                    case "Large":
                    case "Medium":
                    case "Small":
                        ObjectAsset objectAsset = LevelObjects.getAsset(rayHit.transform);
                        if (objectAsset != null && restriction.ValidObjects.ContainsKey(objectAsset.id))
                        {
                            shouldAllow = true;
                            if (Conf.Debug)
                            {
                                Logger.Log($"RestrictedBarricade was placed on {objectAsset.name}");
                            }
                        }
                        break;
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
