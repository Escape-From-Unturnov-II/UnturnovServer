using SDG.Framework.Devkit;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config.ItemExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    internal class AirdropControler
    {
        private static Dictionary<ushort, AirdropSignal> airdropSignalDict;

        internal static void Init(List<AirdropSignal> airdropSignals)
        {
            airdropSignalDict = createAirdropSignalDictionary(airdropSignals);
        }

        internal static void Cleaup()
        {

        }

        internal static void OnThrowableSpawned(UseableThrowable useable, GameObject throwable)
        {
            if (!airdropSignalDict.TryGetValue(useable.equippedThrowableAsset.id, out AirdropSignal airdropSignal))
            {
                return;
            }
            Logger.Log($"Airdrop signal {useable.equippedThrowableAsset.id} thrown at {throwable.transform.position}");

            var dispatcher = throwable.AddComponent<AirdropDelayDispatcher>();
            dispatcher.delay = airdropSignal.DelayInSec;
            dispatcher.spawnTableId = airdropSignal.SpawnTableId;
            dispatcher.SpawnAirdrop += spawnAirdrop;
        }

        internal static void spawnAirdrop(AirdropDelayDispatcher dispatcher)
        {
            spawnAirdrop(dispatcher.gameObject.transform.position, dispatcher.spawnTableId);
        }
        internal static void spawnAirdrop(Vector3 position, ushort spawnTableId)
        { 
            if(spawnTableId == 0)
            {
                // try get random airdropNode from the map and use its spawnTableId
                if (UnturnedPrivateFields.TryGetAirdropNodes(out var airdropNodes) && airdropNodes.Count() > 0)
                {
                    spawnTableId = airdropNodes[UnityEngine.Random.Range(0, airdropNodes.Count)].id;
                }
                else
                {
                    Logger.LogError("Could not call airdrop! No spawn table selected and no airdrop nodes found");
                    return;
                }
            }

            Logger.Log("Airdrop incomming");
            LevelManager.airdrop(position, spawnTableId, Provider.modeConfigData.Events.Airdrop_Speed);
        }

        private static Dictionary<ushort, AirdropSignal> createAirdropSignalDictionary(List<AirdropSignal> airdropSignals)
        {
            Dictionary<ushort, AirdropSignal> airdropSignalDict = new Dictionary<ushort, AirdropSignal>();
            if (airdropSignals != null)
            {
                foreach (AirdropSignal airdropSingal in airdropSignals)
                {
                    if (airdropSingal.Id == 0)
                    {
                        Logger.LogError($"Airdrop Singal with Id 0 was skipped!");
                        continue;
                    }
                    if(airdropSingal.SpawnTableId != 0)
                    {
                        if (SpawnTableTool.resolve(airdropSingal.SpawnTableId) == 0)
                        {
                            Logger.LogError($"Could not find spawn table with Id {airdropSingal.SpawnTableId}, Airdrop Singal ({airdropSingal.Id}) was skipped!");
                            continue;
                        }
                    }
                    ItemThrowableAsset asset = Assets.find(EAssetType.ITEM, airdropSingal.Id) as ItemThrowableAsset;
                    if (asset == null)
                    {
                        Logger.LogError($"Could not find item with Id {airdropSingal.Id}, it was skipped!");
                        continue;
                    }
                    if(asset.fuseLength < airdropSingal.DelayInSec)
                    {
                        Logger.LogWarning($"Fuse length of {airdropSingal.Id} was smaller than the DelayInSec, airdrop will be called after {asset.fuseLength}s");
                    }
                    if (airdropSignalDict.ContainsKey(airdropSingal.Id))
                    {
                        Logger.LogError($"Item with Id: {airdropSingal.Id} is a duplicate!");
                        continue;
                    }
                    airdropSignalDict.Add(airdropSingal.Id, airdropSingal);
                }
            }
            return airdropSignalDict;
        }

        internal class AirdropDelayDispatcher : MonoBehaviour
        {
            internal event Action<AirdropDelayDispatcher> SpawnAirdrop;
            internal float delay;
            internal ushort spawnTableId;
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
                    SpawnAirdrop?.Invoke(this);
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
                SpawnAirdrop?.Invoke(this);
            }
        }
    }
}
