using Rocket.Core.Assets;
using Rocket.Core.Steam;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Hideout;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    internal class BarricadeHelper
    {
        static bool Debug = true;
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
        internal static bool tryGetBarricadeDrop(Transform barricade, out BarricadeDrop barricadeDrop)
        {
            barricadeDrop = null;
            if (!BarricadeManager.tryGetRegion(barricade, out _, out _, out _, out var barricadeRegion))
                return false;

            barricadeDrop = barricadeRegion.FindBarricadeByRootTransform(barricade);
            if(barricadeDrop == null)
                return false;

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
            if (!tryPlaceBarricade(barricadeWrapper.id, position, rotation, playerId, CSteamID.Nil, barricadeWrapper.state, out Transform transform))
            {
                return false;
            }
            switch (barricadeWrapper.barricadeType)
            {
                case EBuild.STORAGE:
                case EBuild.STORAGE_WALL:
                    break;
                case EBuild.FARM:
                    // handle offline groth
                    break;
                case EBuild.GENERATOR:
                case EBuild.OIL:
                    // handle offline burn / generation           
                    break;
            }
            return true;
        }
        internal static BarricadeWrapper getBarricadeWrapper(BarricadeDrop drop, Vector3 position, Quaternion rotation)
        {
            var data = drop.GetServersideData(); 
            BarricadeWrapper barricadeWrapper = new BarricadeWrapper(drop.asset.build, drop.asset.id, position, rotation, data?.barricade?.state);
            switch (drop.asset.build)
            {
                case EBuild.STORAGE:
                case EBuild.STORAGE_WALL:
                    if (!tryGetStoredItems(drop, out List<ItemJarWrapper> storedItems, true))
                    {
                        Logger.LogError($"Could not get storedItems from {drop.asset.id}");
                        break;
                    }
                    barricadeWrapper.items = storedItems;
                    break;
                case EBuild.FARM:
                    if (!tryGetPlantedOfFarm(drop, out uint planted))
                    {
                        Logger.LogError($"Could not get planted from {drop.asset.id}");
                        break;
                    }
                    barricadeWrapper.planted = planted;
                    break;
                case EBuild.GENERATOR:
                case EBuild.OIL:
                case EBuild.BARREL_RAIN:
                case EBuild.TANK:
                    if (!tryGetStoredLiquid(drop, out ushort amount))
                    {
                        Logger.LogError($"Could not get stored liquid from {drop.asset.id}");
                        break;
                    }
                    barricadeWrapper.storedLiquid = amount;
                    break;
            }
            return barricadeWrapper;
        }
        internal static bool tryPlaceBarricade(ushort assetId, Vector3 pos, Quaternion rotation, CSteamID owner, CSteamID group, byte[] state, out Transform placedBarricade)
        {
            placedBarricade = null;
            ItemBarricadeAsset asset = (Assets.find(EAssetType.ITEM, assetId) as ItemBarricadeAsset);
            if (asset == null)
            {
                Logger.LogError($"Could not place barricade: unknown asset [{assetId}]!");
                return false;
            }

            Barricade barricade = new Barricade(asset);
            if(state == null || state.Length <= 0)
            {
                setInitialState(barricade, asset, owner, group);
            }
            else
            {
                barricade.state = state;
            }
            
            placedBarricade = BarricadeManager.dropNonPlantedBarricade(barricade, pos, rotation, owner.m_SteamID, group.m_SteamID);

            if(placedBarricade == null)
            {
                Logger.LogError($"Could not place barricade [{assetId}] at {pos}!");
                return false;
            }
            return true;
        }
        internal static bool tryGetStoredLiquid(BarricadeDrop drop, out ushort amount)
        {
            amount = 0;
            switch (drop.asset.build)
            {
                case EBuild.GENERATOR:
                    var gen = drop.model.GetComponent<InteractableGenerator>();
                    if (gen == null)
                        break;
                    amount = gen.fuel;
                    return true;
                case EBuild.OIL:
                    var oil = drop.model.GetComponent<InteractableOil>();
                    if (oil == null)
                        break;
                    amount = oil.fuel;
                    return true;
                case EBuild.BARREL_RAIN:
                    var barrel = drop.model.GetComponent<InteractableRainBarrel>();
                    if (barrel == null)
                        break;
                    amount = (ushort)(barrel.isFull ? 1 : 0);
                    return true;
                case EBuild.TANK:
                    var tank = drop.model.GetComponent<InteractableTank>();
                    if (tank == null)
                        break;
                    amount = tank.amount;
                    return true;
            }

            Logger.LogError($"Tried to get stored liquid of invalid barricade");
            return false;
        }
        internal static bool tryGetPlantedOfFarm(BarricadeDrop drop, out uint planted)
        {
            planted = 0;

            InteractableFarm farm = drop.model.GetComponent<InteractableFarm>();
            if (farm == null)
                return false;

            planted = farm.planted;
            return true;
        }
        internal static bool tryGetStoredItems(BarricadeDrop drop, out List<ItemJarWrapper> storedItems, bool remove = false)
        {
            storedItems = new List<ItemJarWrapper>();
            if (drop == null)
                return false;
            InteractableStorage storage = drop.model.GetComponent<InteractableStorage>();
            if(storage == null)
                return false;

            int i = 0;
            while (storage.items.items.Count > i)
            {
                storedItems.Add(new ItemJarWrapper(storage.items.items[i]));
                if (remove)
                {
                    storage.items.removeItem(0);
                }
                else
                {
                    i++;
                }
            }
            return true;
        }
        internal static bool tryAddItems(Transform barricade, List<ItemJarWrapper> items)
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
                storage.items.items.Add(item.itemJar);
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
        internal static bool tryUpdateStoredLiquid(Transform barricade, EBuild barricadeType, ushort amount)
        {
            if (barricade == null)
            {
                return false;
            }
            switch (barricadeType)
            {
                case EBuild.GENERATOR:
                    BarricadeManager.sendFuel(barricade, amount);
                    return true;
                case EBuild.OIL:
                    BarricadeManager.sendOil(barricade, amount);
                    break;
                case EBuild.BARREL_RAIN:
                    BarricadeManager.updateRainBarrel(barricade, amount > 0, true);
                    break;
                case EBuild.TANK:
                    var tank = barricade.GetComponent<InteractableTank>();
                    if (tank == null)
                        goto default;
                    tank.ServerSetAmount(amount);
                    break;
                default:
                    Logger.LogError($"Tried to update stored liquid of invalid barricade");
                    return false;

            }
            return true;
        }
        internal static void setInitialState(Barricade barricade, ItemBarricadeAsset itemBarricadeAsset, CSteamID owner, CSteamID group)
        {
            switch (itemBarricadeAsset.build)
            {
                case EBuild.DOOR:
                case EBuild.GATE:
                case EBuild.SHUTTER:
                case EBuild.SIGN:
                case EBuild.SIGN_WALL:
                case EBuild.NOTE:
                case EBuild.HATCH:
                case EBuild.STORAGE:
                case EBuild.STORAGE_WALL:
                case EBuild.MANNEQUIN:
                case EBuild.SENTRY:
                case EBuild.SENTRY_FREEFORM:
                case EBuild.LIBRARY:
                    BitConverter.GetBytes(owner.m_SteamID).CopyTo(barricade.state, 0);
                    BitConverter.GetBytes(group.m_SteamID).CopyTo(barricade.state, 8);
                    break;
                case EBuild.BED:
                    BitConverter.GetBytes(CSteamID.Nil.m_SteamID).CopyTo(barricade.state, 0);
                    break;
                case EBuild.FARM:
                    // set planted is curretnly ignored as it will be set later anyways
                    //BitConverter.GetBytes(Provider.time - (uint)((float)((ItemFarmAsset)base.player.equipment.asset).growth * (base.player.skills.mastery(2, 5) * 0.25f))).CopyTo(barricade.state, 0);
                    break;
                case EBuild.TORCH:
                case EBuild.CAMPFIRE:
                case EBuild.OVEN:
                case EBuild.SPOT:
                case EBuild.SAFEZONE:
                case EBuild.OXYGENATOR:
                case EBuild.CAGE:
                case EBuild.GENERATOR:
                    barricade.state[0] = 1;
                    break;
                case EBuild.STEREO:
                    barricade.state[16] = 100;
                    break;
            }
        }
    }
}
