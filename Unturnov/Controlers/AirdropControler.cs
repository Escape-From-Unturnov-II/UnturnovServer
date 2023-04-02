using SDG.Framework.Devkit;
using SDG.Unturned;
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
        private static float AirdropDelay = 10;
        internal static void OnThrowableSpawned(UseableThrowable useable, GameObject throwable)
        {
            Logger.Log($"useable {useable.equippedThrowableAsset.id} thrown at {throwable.transform.position}");

            var dispatcher = throwable.AddComponent<AirdropDelayDispatcher>();
            dispatcher.delay = AirdropDelay;
            dispatcher.SpawnAirdrop += spawnAirdrop;
        }

        public static void spawnAirdrop(GameObject gameObject)
        {
            spawnAirdrop(gameObject.transform.position);
        }
        public static void spawnAirdrop(Vector3 position)
        {
            ushort id = 1;
            if (UnturnedPrivateFields.TryGetAirdropNodes(out var airdropNodes) && airdropNodes.Count() > 0)
            {
                id = airdropNodes[UnityEngine.Random.Range(0, airdropNodes.Count)].id;
            }
            Logger.Log("Airdrop incomming");
            LevelManager.airdrop(position, id, Provider.modeConfigData.Events.Airdrop_Speed);
        }

        
        internal class AirdropDelayDispatcher : MonoBehaviour
        {
            public event Action<GameObject> SpawnAirdrop;
            public float delay;
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
