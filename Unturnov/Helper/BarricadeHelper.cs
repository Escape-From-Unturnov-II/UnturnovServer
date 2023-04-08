using Rocket.Core.Assets;
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
        internal static bool trySalvageBarricade(BarricadeDrop drop, Player player = null)
        {
            if (!BarricadeManager.tryGetRegion(drop.model, out var x, out var y, out var plant, out var region))
            {
                Logger.LogError($"Could not find region for Barricade {drop.asset.id} at {drop.model.position}");
                return false;
            }

            var serversideData = drop.GetServersideData();
            if (serversideData.barricade.health >= drop.asset.health)
            {
                Item item = new Item(serversideData.barricade.asset.id, EItemOrigin.NATURE);
                if (player != null)
                {
                    player.inventory.forceAddItem(item, auto: true);
                }
                else
                {
                    ItemManager.dropItem(item, drop.model.position, playEffect: false, isDropped: true, wideSpread: true);
                }
            }
            else if (drop.asset.isSalvageable)
            {
                ItemAsset itemAsset = drop.asset.FindSalvageItemAsset();
                if (itemAsset != null)
                {
                    Item item = new Item(itemAsset, EItemOrigin.NATURE);
                    if (player != null)
                    {
                        player.inventory.forceAddItem(item, auto: true);
                    }
                    else
                    {
                        ItemManager.dropItem(item, drop.model.position, playEffect: false, isDropped: true, wideSpread: true);
                    }
                }
            }

            BarricadeManager.destroyBarricade(drop, x, y, plant);
            return true;
        }
        internal static bool tryPlaceBarricadeWrapper(BarricadeWrapper barricadeWrapper, CSteamID playerId, Vector3 position, Quaternion rotation)
        {
            if (!tryPlaceBarricade(barricadeWrapper.id, position, rotation, playerId, CSteamID.Nil, out Transform transform))
            {
                return false;
            }
            switch (barricadeWrapper.barricadeType)
            {
                case EBuild.STORAGE:
                case EBuild.STORAGE_WALL:
                    tryAddItems(transform, barricadeWrapper.items);
                    break;
                case EBuild.FARM:
                    tryUpdatePlanted(transform, barricadeWrapper.planted);
                    break;
            }
            return true;
        }
        internal static BarricadeWrapper getBarricadeWrapper(BarricadeDrop drop, Vector3 position, Quaternion rotation)
        {
            uint planted = 0;
            List<ItemJar> storedItems = null;

            switch (drop.asset.build)
            {
                case EBuild.STORAGE:
                case EBuild.STORAGE_WALL:
                    if (!tryGetStoredItems(drop, out storedItems))
                    {
                        Logger.LogError($"Could not get storedItems from {drop.asset.id}");
                    }
                    break;
                case EBuild.FARM:
                    if (!tryGetPlantedOfFarm(drop, out planted))
                    {
                        Logger.LogError($"Could not get planted from {drop.asset.id}");
                    }
                    break;
            }

            return new BarricadeWrapper(drop.asset.build, drop.asset.id, position, rotation, storedItems, planted);
        }
        internal static bool tryPlaceBarricade(ushort assetId, Vector3 pos, Quaternion rotation, CSteamID owner, CSteamID group, out Transform placedBarricade)
        {
            placedBarricade = null;
            ItemBarricadeAsset asset = (Assets.find(EAssetType.ITEM, assetId) as ItemBarricadeAsset);
            if (asset == null)
            {
                Logger.LogError($"Could not place barricade: unknown asset [{assetId}]!");
                return false;
            }

            placedBarricade = BarricadeManager.dropNonPlantedBarricade(new Barricade(asset), pos, rotation, owner.m_SteamID, group.m_SteamID);

            if(placedBarricade == null)
            {
                Logger.LogError($"Could not place barricade [{assetId}] at {pos}!");
                return false;
            }
            return true;
        }
        internal static bool tryGetPlantedOfFarm(BarricadeDrop drop, out uint planted)
        {
            planted = 0;

            InteractableFarm farm = drop.model.GetComponent<InteractableFarm>();
            if (farm == null)
                return false;

            planted = farm.planted;
            Logger.Log($"groth is new {Provider.time > planted} planted {planted} state: {Provider.time - planted} of {farm.growth}");
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
        internal static bool tryUpdatePlanted(Transform barricade, uint planted)
        {
            if (barricade == null)
            {
                return false;
            }
            InteractableFarm farm = barricade.GetComponent<InteractableFarm>();
            if (farm == null)
            {
                Logger.LogError($"Tried to update planted of non storage barricade");
                return false;
            }
            
            farm.updatePlanted(planted);
            BarricadeManager.updateFarm(farm.transform, planted, true);
            return true;
        }
    }
}
