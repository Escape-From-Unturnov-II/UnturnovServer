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
        internal Vector3[] bounds;
        private Vector3 origin;
        private Vector3 rotation;
        private List<BarricadeStruct> barricades = new List<BarricadeStruct>();

        internal Hideout(Vector3 origin, float rotation)
        {
            this.origin = origin;
            this.rotation = new Vector3(0,rotation);

            Vector3[] bounds = calcBounds(origin);
            setBounds(bounds);
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
        internal void restoreBarricade(BarricadeWrapper barricade, CSteamID playerId)
        { 
            BarricadeHelper.tryPlaceBarricade(barricade.id, barricade.location, barricade.rotation, playerId, CSteamID.Nil);
        }
        internal BarricadeWrapper convertToRelativePosition(BarricadeWrapper barricade)
        {
            barricade.location = barricade.location - origin;
            barricade.rotation = barricade.rotation - rotation;
            return barricade;
        }
        internal BarricadeWrapper convertToAbsolutePosition(BarricadeWrapper barricade)
        {
            barricade.location = barricade.location + origin;
            barricade.rotation = barricade.rotation + rotation;
            return barricade;
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

       

        struct BarricadeStruct
        {
            ushort id;
            Vector3 pos;
            Vector3 rot;

            internal BarricadeStruct(ushort id, Vector3 pos, Vector3 rot)
            {
                this.id = id;
                this.pos = pos;
                this.rot = rot;
            } 
            
        }
    }
}
