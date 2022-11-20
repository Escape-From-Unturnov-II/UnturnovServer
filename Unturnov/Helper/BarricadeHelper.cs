using SDG.Unturned;
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
                    Logger.Log($"found drop with id {id} distance {Vector3.Distance(drop.model.transform.position, location)}");
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

        internal static bool tryPlaceBarricade(ushort assetId, Vector3 pos, Vector3 rotation, CSteamID owner, CSteamID group)
        {
            ItemBarricadeAsset asset = (Assets.find(EAssetType.ITEM, assetId) as ItemBarricadeAsset);
            if (asset == null)
            {
                Logger.LogError($"Place barricade: unknown asset [{assetId}]!");
                return false;
            }

            Barricade barricade = new Barricade(asset);
            Transform barricadeTransform = BarricadeManager.dropBarricade(barricade, null, pos, rotation.x, rotation.y, rotation.z, owner.m_SteamID, group.m_SteamID);

            if(barricadeTransform == null)
            {
                Logger.LogError($"Could not place barricade [{assetId}]!");
                return false;
            }

            return true;
        }
    }
}
