using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Models
{
    internal class Hideout
    {
        internal CSteamID owner;
        internal Vector3[] bounds;
        internal Vector3 origin;
        internal Vector3 rotation;

        private Vector3 hideoutDimensions = new Vector3(11, 5, 8);
        private List<BarricadeDrop> barricades = new List<BarricadeDrop>();

        internal Hideout(Vector3 origin, float rotation)
        {
            owner = CSteamID.Nil;
            this.origin = origin;
            this.rotation = new Vector3(0, rotation, 0);

            Vector3[] bounds = calcBounds(origin);
            setBounds(bounds);
        }
        internal void claim(CSteamID newOwner)
        {
            owner = newOwner;
            barricades.Clear();
        }
        internal void free()
        {
            owner = CSteamID.Nil;
        }
        internal int getBarricadeCount()
        {
            return barricades.Count();
        }
        internal void addBarricade(BarricadeDrop drop)
        {
            barricades.Add(drop);
        }
        internal void removeBarricade(BarricadeDrop drop)
        {
            if(!barricades.Remove(drop))
            {
                Logger.LogError($"could not find destroyed barricade {drop.asset.id} in hideout of {owner}");
                return;
            };
        }
        /*
         * tries to clears all barricades and gives the successfully cleared ones with relative position and rotation to the hideout origin
         * returns true if all barricades where succesfully removed
         */
        internal bool clearBarricades(out List<BarricadeWrapper> removedBarricades)
        {
            removedBarricades = new List<BarricadeWrapper>();
            bool success = true;
            while (barricades.Count > 0)
            {
                var current = barricades[0];
                BarricadeHelper.tryGetStoredItems(current, out var storedItems);
                barricades.RemoveAt(0);

                if (!UnturnedPrivateFields.TryGetServersideData(current, out BarricadeData data))
                {
                    Logger.LogWarning($"Could not get server side data for {current.asset.id} at {current.model.position}, canceled clearing hideout");
                    return false;
                }
                if (!BarricadeHelper.tryDestroyBarricade(current.model.position, current.asset.id))
                {
                    Logger.LogWarning($"Barricade {current.asset.id} of {owner} at {current.model.position} could not be destroyed!");
                    success = false;
                    continue;
                }
                convertToRelative(data.point, new Vector3(data.angle_x, data.angle_y, data.angle_z), out Vector3 relPosition, out Vector3 relRotation);
                removedBarricades.Add(new BarricadeWrapper(current.asset.id, relPosition, relRotation, storedItems));
            }
            return success;
        }
        internal void restoreBarricades(List<BarricadeWrapper> barricades, CSteamID playerId)
        {
            foreach (BarricadeWrapper barricade in barricades)
            {
                convertToAbsolute(barricade.location, barricade.rotation, out Vector3 absPosition, out Vector3 absRotation);
                BarricadeHelper.tryPlaceBarricade(barricade.id, absPosition, absRotation, playerId, CSteamID.Nil, out Transform transform);
                // barricade drops will be automatically added when succesesfully placed
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool isInBounds(Vector3 point)
        {
            if (bounds[0].x <= point.x && point.x <= bounds[1].x &&
                bounds[0].y <= point.y && point.y <= bounds[1].y &&
                bounds[0].z <= point.z && point.z <= bounds[1].z)
            {
                return true;
            }
            return false;
        }
        private Vector3[] calcBounds(Vector3 point)
        {
            Vector3[] bounds = new Vector3[2];

            bounds[0] = point;
            bounds[1] = point + Quaternion.Euler(rotation) * hideoutDimensions;

            return bounds;
        }

        private void setBounds(Vector3[] bounds)
        {
            this.bounds = new Vector3[2];
            if (bounds.Length < 2) return;
            if (bounds.Length > 2)
            {
                Logger.LogWarning("Hideout bounds only allow 2 points!");
            }
            Vector3 lowerBound = new Vector3();
            Vector3 upperBound = new Vector3();

            getMinMax(bounds[0].x, bounds[1].x, out lowerBound.x, out upperBound.x);
            getMinMax(bounds[0].y, bounds[1].y, out lowerBound.y, out upperBound.y);
            getMinMax(bounds[0].z, bounds[1].z, out lowerBound.z, out upperBound.z);

            this.bounds[0] = lowerBound;
            this.bounds[1] = upperBound;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void getMinMax(float valA , float valB, out float min, out float max)
        {
            if (valA > valB)
            {
                max = valA;
                min = valB;
            }
            else
            {
                max = valB;
                min = valA;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void convertToRelative(Vector3 location, Vector3 rotation, out Vector3 relativePosition, out Vector3 relativeRotation)
        {
            relativePosition = Quaternion.Euler(this.rotation) * (location - origin);
            relativeRotation = rotation - this.rotation;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void convertToAbsolute(Vector3 location, Vector3 rotation, out Vector3 absolutePosition, out Vector3 absoluteRotation)
        {
            absolutePosition = origin + Quaternion.Euler(this.rotation) * location;
            absoluteRotation = rotation + this.rotation;
        }
    }
}
