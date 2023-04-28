using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Models.Hideout
{
    internal class Hideout : MonoBehaviour
    {
        internal CSteamID owner;
        internal Vector3[] bounds;

        internal Vector3 originPosition;
        internal Vector3 originRotationEuler;
        internal Quaternion originRotationQuanternion;

        internal bool ready = false;
        private bool debug = true;
        private Vector3 hideoutDimensions = new Vector3(11, 5, 8);
        private List<BarricadeDrop> barricades = new List<BarricadeDrop>();

        internal Hideout()
        {
            
        }
        internal void Initialize(Vector3 origin, float rotation)
        {
            owner = CSteamID.Nil;
            originPosition = origin;
            originRotationEuler = new Vector3(0, rotation, 0);
            originRotationQuanternion = Quaternion.Euler(originRotationEuler);

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
            ready = false;
            owner = CSteamID.Nil;
        }
        internal int getBarricadeCount()
        {
            return barricades.Count();
        }
        internal void addBarricade(BarricadeDrop drop)
        {
            barricades.Add(drop);
            BarricadeHelper.tryGetPlantedOfFarm(drop, out uint planted);
        }
        internal void removeBarricade(BarricadeDrop drop)
        {
            if(!barricades.Remove(drop))
            {
                Logger.LogError($"could not find destroyed barricade {drop.asset.id} in hideout of {owner}");
                return;
            };
        }
        internal List<BarricadeWrapper> getBarricades()
        {
            var barricadesWrappers = new List<BarricadeWrapper>();
            foreach (var barricade in barricades)
            {
                BarricadeData data = barricade.GetServersideData();
                convertToRelative(data.point, barricade.model.rotation, out Vector3 relPosition, out Quaternion relRotation);
                barricadesWrappers.Add(BarricadeHelper.getBarricadeWrapper(barricade, relPosition, relRotation));
            }
            return barricadesWrappers;
        }
        /*
         * tries to clears all barricades and gives the successfully cleared ones with relative position and rotation to the hideout origin
         * returns true if all barricades where succesfully removed
         */
        internal bool clearBarricades(out List<BarricadeWrapper> removedBarricades)
        {
            removedBarricades = new List<BarricadeWrapper>();
            int skippCounter = 0;
            
            while (barricades.Count > skippCounter)
            {
                var current = barricades[0];

                BarricadeData data = current.GetServersideData();
                convertToRelative(data.point, current.model.rotation, out Vector3 relPosition, out Quaternion relRotation);
                var currentWrapper = BarricadeHelper.getBarricadeWrapper(current, relPosition, relRotation);
                if (!BarricadeHelper.tryDestroyBarricade(current.model))
                {
                    Logger.LogWarning($"Barricade {current.asset.id} of {owner} at {current.model.position} could not be destroyed!");
                    skippCounter++;
                    continue;
                }
                removedBarricades.Add(currentWrapper);
            }
            return skippCounter == 0;
        }
        
        internal void restoreBarricades(List<BarricadeWrapper> barricades, CSteamID playerId)
        {
            if (debug)
                Logger.Log($"Started restorring barricades {Provider.time}");

            StartCoroutine("restoreBarricadesInner", new RestoreWrapper(barricades, playerId));
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
            bounds[1] = point + originRotationQuanternion * hideoutDimensions;

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
        public void convertToRelative(Vector3 location, Quaternion rotation, out Vector3 relativePosition, out Quaternion relativeRotation)
        {
            relativePosition = originRotationQuanternion * (location - originPosition);
            relativeRotation = Quaternion.Euler(-originRotationEuler) * rotation;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void convertToAbsolute(Vector3 location, Quaternion rotation, out Vector3 absolutePosition, out Quaternion absoluteRotation)
        {
            absolutePosition = originPosition + originRotationQuanternion * location;
            absoluteRotation = Quaternion.Euler(originRotationEuler) * rotation;
        }

        private IEnumerator restoreBarricadesInner(RestoreWrapper restoreWrapper)
        {
            foreach (BarricadeWrapper barricade in restoreWrapper.barricades)
            {
                convertToAbsolute(barricade.position, barricade.rotation, out Vector3 absPosition, out Quaternion absRotation);
                BarricadeHelper.tryPlaceBarricadeWrapper(barricade, restoreWrapper.playerId, absPosition, absRotation);
                // barricade drops will be automatically added when succesesfully placed
                yield return null;
            };
            ready = true;
            if(debug)
                Logger.Log($"Finished restorring barricades {Provider.time}");
        }
        internal struct RestoreWrapper
        {
            internal List<BarricadeWrapper> barricades;
            internal CSteamID playerId;

            internal RestoreWrapper(List<BarricadeWrapper> barricades, CSteamID playerId)
            {
                this.barricades = barricades;
                this.playerId = playerId;
            }
        }

    }
}
