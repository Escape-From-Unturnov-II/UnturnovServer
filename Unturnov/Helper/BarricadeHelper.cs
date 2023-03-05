using SDG.Unturned;
using SpeedMann.Unturnov.Models;
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
    internal class BarricadeHelper
    {
        internal static bool tryDestroyBarricade(Vector3 location , ushort id)
        {
            if(!Regions.tryGetCoordinate(location, out byte x, out byte y))
            {
                Logger.LogError($"destroy barricade: could not find coordinates location: {location}");
                return false;
            }

            if (!BarricadeManager.tryGetRegion(x, y, ushort.MaxValue, out BarricadeRegion region))
            {
                Logger.LogError($"destroy barricade: could not find region x: {x} y: {y}");
                return false;
            }
           
            foreach(BarricadeDrop drop in region.drops)
            {
                if(drop.asset.id == id)
                {
                    if(Vector3.Distance(drop.model.transform.position, location) <= 0.01)
                    {
                        BarricadeManager.destroyBarricade(drop, x, y, ushort.MaxValue);
                        return true;
                    }
                }
            }
            Logger.LogError($"destroy barricade: could not find barricade [{id}]");
            return false;
        }
        internal static bool tryDestroyBarricade(Transform transform)
        {
            
            if (!BarricadeManager.tryGetRegion(transform, out byte x, out byte y, out ushort plant, out BarricadeRegion barricadeRegion))
            {
                Logger.LogError($"Error destroying barricade");
                return false;
            }

            BarricadeDrop barricadeDrop = barricadeRegion.FindBarricadeByRootTransform(transform);
            BarricadeManager.destroyBarricade(barricadeDrop, x, y, plant);

            return true;
        }
        internal static bool tryPlaceBarricade(ushort assetId, Vector3 pos, Vector3 rotation, CSteamID owner, CSteamID group, out Transform placedBarricade, List<ItemJar> items = null)
        {
            placedBarricade = null;
            ItemBarricadeAsset asset = (Assets.find(EAssetType.ITEM, assetId) as ItemBarricadeAsset);
            if (asset == null)
            {
                Logger.LogError($"Place barricade: unknown asset [{assetId}]!");
                return false;
            }

            placedBarricade = BarricadeManager.dropBarricade(new Barricade(asset), null, pos, rotation.x, rotation.y, rotation.z, owner.m_SteamID, group.m_SteamID);

            if(placedBarricade == null)
            {
                Logger.LogError($"Could not place barricade [{assetId}] at {pos}!");
                return false;
            }

            tryAddItems(placedBarricade, items);

            return true;
        }
        internal static bool tryGetPlantedOfFarm(BarricadeDrop drop, out uint planted)
        {
            planted = 0;

            InteractableFarm farm = drop.model.GetComponent<InteractableFarm>();
            if (farm == null)
                return false;

            Logger.Log($"groth state: {Provider.time - planted} of {farm.growth}");
            planted = farm.planted;
            return true;
        }
        internal static bool tryGetStoredItems(BarricadeDrop drop, out List<ItemJar> storedItems)
        {
            storedItems = new List<ItemJar>();
            InteractableStorage storage = drop.model.GetComponent<InteractableStorage>();
            if(storage == null)
                return false;

            storedItems = storage.items.items;
            return true;
        }
        internal static bool tryAddItems(Transform barricade, List<ItemJar> items)
        {
            if(items == null || barricade == null)
            {
                return false;
            }
            InteractableStorage storage = barricade.GetComponent<InteractableStorage>();
            if (storage == null)
            {
                Logger.LogError($"Tried to add {items.Count} items to non storage barricade");
                return false;
            }
            foreach (var item in items) 
            {
                storage.items.items.Add(item);
            }

            return true;
        }
    }
}
