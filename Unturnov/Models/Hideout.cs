using SDG.Unturned;
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
        public CSteamID owner { get; private set; }
        private Vector3 origin;
        private float rotation;
        private Vector3[] bounds;
        private List<BarricadeStruct> barricades = new List<BarricadeStruct>();

        internal Hideout(Vector3 origin, float rotation, Vector3[] bounds)
        {
            this.owner = CSteamID.Nil;
            this.origin = origin;
            this.rotation = rotation;
            setBounds(bounds);
        }

        internal void addBarricade(ushort id, Vector3 pos, Vector3 rot)
        {
            barricades.Add(new BarricadeStruct(id, pos, rot));
        }

        internal void saveBarricades()
        {

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
        internal bool isOwned()
        {
            return owner != CSteamID.Nil;
        }
        private void restoreBarricades()
        {

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
