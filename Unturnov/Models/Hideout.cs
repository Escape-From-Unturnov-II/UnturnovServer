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
        private List<BarricadeWrapper> barricades = new List<BarricadeWrapper>();

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
        internal void addBarricade(CSteamID playerId, ItemBarricadeAsset barricade, Vector3 location, Vector3 rotation)
        {
            barricades.Add(new BarricadeWrapper(barricade.id, location, rotation));
        }
        internal List<BarricadeWrapper> clearBarricades()
        {
            foreach (BarricadeWrapper barricade in barricades)
            {
                if (!BarricadeHelper.tryDestroyBarricade(barricade.location, barricade.id))
                {
                    Logger.LogWarning($"Barricade {barricade.id} of {owner} at {barricade.location} could not be destroyed");
                }

                barricade.convertToRelative(origin, rotation);
            }
            return barricades;
        }
        internal void restoreBarricades(List<BarricadeWrapper> barricades, CSteamID playerId)
        {
            foreach (BarricadeWrapper barricade in barricades)
            {
                barricade.convertToAbsolute(origin, rotation);
                BarricadeHelper.tryPlaceBarricade(barricade.id, barricade.location, barricade.rotation, playerId, CSteamID.Nil);
            };
            // barricade will be automatically added when succesesfully placed
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
            float height = 5;
            float length = 8;
            float width = 10;
            Vector3 offset = new Vector3(width, height, length);

            Vector3[] bounds = new Vector3[2];

            bounds[0] = point;
            bounds[1] = point + Quaternion.Euler(rotation) * offset;

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
    }
}
