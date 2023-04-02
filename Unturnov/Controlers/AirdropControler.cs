using SDG.Framework.Devkit;
using SDG.Unturned;
using SpeedMann.Unturnov.Models.Config.ItemExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    internal class AirdropControler
    {
        private static Dictionary<ushort, AirdropSignal> airdropSignalDict;
        private static float AirdropDelay = 10;
        private static ushort DefaultSpawnTableId = 1;

        internal static void Init(List<AirdropSignal> airdropSignals)
        {
            airdropSignalDict = Unturnov.createDictionaryFromItemExtensions(airdropSignals);
            //TODO: add better checks (check if spawn table id is valid and delay < fuse time)
        }

        internal static void OnThrowableSpawned(UseableThrowable useable, GameObject throwable)
        {
            Logger.Log($"useable {useable.equippedThrowableAsset.id} thrown at {throwable.transform.position}");

            var dispatcher = throwable.AddComponent<AirdropDelayDispatcher>();
            dispatcher.delay = AirdropDelay;
            dispatcher.SpawnAirdrop += spawnAirdrop;
        }

        internal static void spawnAirdrop(GameObject gameObject)
        {
            spawnAirdrop(gameObject.transform.position);
        }
        internal static void spawnAirdrop(Vector3 position)
        {
            // used spawnTable
            ushort spawnTableId = DefaultSpawnTableId;
            if (UnturnedPrivateFields.TryGetAirdropNodes(out var airdropNodes) && airdropNodes.Count() > 0)
            {
                spawnTableId = airdropNodes[UnityEngine.Random.Range(0, airdropNodes.Count)].id;
            }
            if(spawnTableId == 0)
            {
                Logger.LogError("Could not call airdrop! No spawn table selected and no airdrop nodes found");
                return;
            }

            Logger.Log("Airdrop incomming");
            LevelManager.airdrop(position, spawnTableId, Provider.modeConfigData.Events.Airdrop_Speed);
        }

        
        internal class AirdropDelayDispatcher : MonoBehaviour
        {
            internal event Action<GameObject> SpawnAirdrop;
            internal float delay;
            private bool invoked = false;
            internal void Start()
            {
                StartCoroutine("delayAirdrop");
            }
            internal void OnDestroy()
            {
                StopCoroutine("delayAirdrop");
                if (!invoked)
                {
                    invoked = true;
                    SpawnAirdrop?.Invoke(gameObject);
                }
            }

            private IEnumerator delayAirdrop()
            {
                Vector3 position = gameObject.transform.position;
                yield return new WaitForSecondsRealtime(delay);
                if (gameObject != null)
                {
                    position = gameObject.transform.position;
                }
                invoked = true;
                SpawnAirdrop?.Invoke(gameObject);
            }
        }
    }
}
