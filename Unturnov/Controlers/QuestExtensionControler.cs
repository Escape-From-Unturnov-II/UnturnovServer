using Rocket.Core.Steam;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using SpeedMann.Unturnov.Models.Config.QuestExtensions;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Controlers
{
    internal class QuestExtensionControler
    {
        private static Dictionary<CSteamID, GenericOccurrence<Zombie>> lastDamagedZombie;
        private static Dictionary<CSteamID, GenericOccurrence<Animal>> lastDamagedAnimal;
        private static Dictionary<CSteamID, Dictionary<ushort, Coroutine>> activeCooldownDict;

        internal static void Init()
        {
            lastDamagedZombie = new Dictionary<CSteamID, GenericOccurrence<Zombie>>();
            lastDamagedAnimal = new Dictionary<CSteamID, GenericOccurrence<Animal>>();
            activeCooldownDict = new Dictionary<CSteamID, Dictionary<ushort, Coroutine>>();
        }
        internal static void Cleanup()
        {
            lastDamagedZombie.Clear();
            lastDamagedAnimal.Clear();
            foreach (var playerCooldownEntry in activeCooldownDict)
            {
                Player player = PlayerTool.getPlayer(playerCooldownEntry.Key);
                foreach (var cooldownEntry in playerCooldownEntry.Value)
                {
                   if(cooldownEntry.Value != null)
                   {
                        player.StopCoroutine(cooldownEntry.Value);
                   }
                }
            }
            activeCooldownDict.Clear();
        }
        internal static void OnPlayerDisconected(UnturnedPlayer player)
        {
            lastDamagedAnimal.Remove(player.CSteamID);
            lastDamagedZombie.Remove(player.CSteamID);
            activeCooldownDict.Remove(player.CSteamID);
        }
        internal static void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (!wasPvpDeath(player.Player, out murderer)) return;
            // was pvp kill
        }
        internal static void OnFishCaught(UnturnedPlayer player)
        {
            // caught fish
        }
        internal static void OnResourceDamage(Transform resource, ushort damage, UnturnedPlayer player)
        {
            if (Regions.tryGetCoordinate(resource.position, out byte x, out byte y))
            {
                List<ResourceSpawnpoint> list = LevelGround.trees[x, y];
                ushort num2 = 0;
                while (num2 < list.Count)
                {
                    if (resource == list[num2].model
                        && !list[num2].isDead
                        && list[num2].canBeDamaged
                        && list[num2].health <= damage)
                    {
                        // harvested resource
                        return;
                    }
                    num2 += 1;
                }
            }
        }
        internal static void OnZombieDamage(UnturnedPlayer player, ref DamageZombieParameters parameters, ref bool canDamage)
        {
            if (parameters.zombie == null) return;

            GenericOccurrence<Zombie> occurrence = new GenericOccurrence<Zombie>(Time.realtimeSinceStartup, parameters.zombie);

            if (lastDamagedZombie.ContainsKey(player.CSteamID))
            {
                lastDamagedZombie[player.CSteamID] = occurrence;
                return;
            }
            lastDamagedZombie.Add(player.CSteamID, occurrence);
        }
        internal static void OnZombieDeath(Zombie zombie)
        {
            UnturnedPlayer player = null;
            float latestOccurence = float.MaxValue;

            foreach (KeyValuePair<CSteamID, GenericOccurrence<Zombie>> entry in lastDamagedZombie)
            {
                float currrentOccurrence = Time.realtimeSinceStartup - entry.Value.lastTime;
                if (entry.Value.occurredEvent.Equals(zombie) && currrentOccurrence < latestOccurence)
                {
                    latestOccurence = currrentOccurrence;
                    player = UnturnedPlayer.FromCSteamID(entry.Key);

                }
            }
            if(player != null)
            {
                // killed zombie
            }
        }
        internal static void OnAnimalDamage(UnturnedPlayer player, ref DamageAnimalParameters parameters, ref bool canDamage)
        {
            if (parameters.animal == null) return;

            GenericOccurrence<Animal> occurrence = new GenericOccurrence<Animal>(Time.realtimeSinceStartup, parameters.animal);
            if (lastDamagedAnimal.ContainsKey(player.CSteamID))
            {
                lastDamagedAnimal[player.CSteamID] = occurrence;
                return;
            }
            lastDamagedAnimal.Add(player.CSteamID, occurrence);
        }
        internal static void OnAnimalDeath(Animal animal)
        {
            UnturnedPlayer player = null;
            float latestOccurence = float.MaxValue;
            foreach (KeyValuePair<CSteamID, GenericOccurrence<Animal>> entry in lastDamagedAnimal)
            {
                float currrentOccurrence = Time.realtimeSinceStartup - entry.Value.lastTime;
                if (entry.Value.occurredEvent.Equals(animal) && currrentOccurrence < latestOccurence)
                {
                    latestOccurence = currrentOccurrence;
                    player = UnturnedPlayer.FromCSteamID(entry.Key);
                }
            }
            if (player != null)
            {
                // killed animal
            }
        }
        
        #region Helper Functions
        public static bool wasPvpDeath(Player player, out CSteamID killer)
        {
            killer = CSteamID.Nil;
            if (!UnturnedPrivateFields.TryGetLastTimeDamaged(player.life, out float lastTimeDamaged)
                || !UnturnedPrivateFields.TryGetRecentKiller(player.life, out CSteamID oponent)
                || !UnturnedPrivateFields.TryGetCombatCooldown(out float combatCooldown))
            {
                Logger.LogError("Could not load private fields for QuestExtension.wasPvpDeath()");
                return false;
            }
            if (oponent != CSteamID.Nil
                && oponent != player.channel.owner.playerID.steamID
                && Time.realtimeSinceStartup - lastTimeDamaged < combatCooldown)
            {
                killer = oponent;
                return true;
            }
            return false;
        }

        public static bool TryStartQuestBasedCooldown(Player player, QuestCooldown questCooldown, float cooldownInMin, ushort controlFlag, short startValue, short stopValue, bool restore = false)
        {
            CSteamID steamId = player.channel.owner.playerID.steamID;

            if (questCooldown == null || !questCooldown.IsValid())
            {
                Logger.LogError($"Could not start cooldown for flag {controlFlag}!");
                player.quests.sendSetFlag(controlFlag, stopValue);
                return false;
            }
            QuestAsset questAsset = Assets.find(EAssetType.NPC, questCooldown.QuestId) as QuestAsset;
            if (questAsset == null)
            {
                Logger.LogError($"Could not find cooldown quest {questCooldown.QuestId} for flag {controlFlag}!");
                player.quests.sendSetFlag(controlFlag, stopValue);
                return false;
            }

            short minValue = 0;
            short secValue = 0;
            float cooldown = cooldownInMin;

            if (restore)
            {
                player.quests.getFlag(questCooldown.MinFlag, out minValue);
                player.quests.getFlag(questCooldown.SecFlag, out secValue);

                cooldown = minValue + (float)secValue / 60;
            }

            if (cooldown <= 0)
            {
                if (restore)
                {
                    Logger.Log($"Reset cooldown for {controlFlag} min {minValue}, sec {secValue} for {steamId}");
                    SetCooldown(player, questCooldown.MinFlag, questCooldown.SecFlag, 0);
                    player.quests.sendSetFlag(controlFlag, stopValue);
                    return true;
                }
                return false;
            }


            Logger.Log($"Started cooldown for {controlFlag}, duration {cooldownInMin}min for {steamId}, restore {restore}");

            Coroutine cooldownCoroutine = player.StartCoroutine(CooldownRoutine(player, questAsset, controlFlag, questCooldown.MinFlag, questCooldown.SecFlag, cooldown, startValue, stopValue));
            AddOrReplaceCooldown(player, controlFlag, cooldownCoroutine);
            return true;
        }
        private static IEnumerator CooldownRoutine(Player player, QuestAsset questAsset, ushort controlFlag, ushort minFlagId, ushort secFlagId, float cooldownInMin, short startValue, short stopValue)
        {
            player.quests.sendSetFlag(controlFlag, startValue);
            short cooldown = (short)Mathf.RoundToInt(cooldownInMin * 60);
            SetCooldown(player, minFlagId, secFlagId, cooldown);
            if (player.quests.GetQuestStatus(questAsset) == ENPCQuestStatus.NONE)
            {
                player.quests.ServerAddQuest(questAsset);
            }
            short minutes = (short)(cooldown / 60);
            short seconds = (short)(cooldown % 60);
            while (cooldown >= 0)
            {
                yield return new WaitForSecondsRealtime(1);
                seconds--;
                if(seconds < 0)
                {
                    minutes--;
                    if (minutes < 0)
                    {
                        break;
                    }
                    player.quests.sendSetFlag(minFlagId, minutes);
                    seconds = 60;
                }
                player.quests.sendSetFlag(secFlagId, seconds);
            }

            player.quests.sendSetFlag(controlFlag, stopValue);
            player.quests.ServerRemoveQuest(questAsset, true);
            if (activeCooldownDict.TryGetValue(player.channel.owner.playerID.steamID, out Dictionary<ushort, Coroutine> activeCooldowns))
            {
                activeCooldowns.Remove(controlFlag);
            }
        }
        
        private static void SetCooldown(Player player, ushort minFlagId, ushort secFlagId, short cooldownInSec)
        {
            short minutes = (short)(cooldownInSec / 60);
            short seconds = (short)(cooldownInSec % 60);
            player.quests.sendSetFlag(minFlagId, minutes);
            player.quests.sendSetFlag(secFlagId, seconds);
        }
        private static void AddOrReplaceCooldown(Player player, ushort controlFlag, Coroutine newCoroutine)
        {
            CSteamID steamId = player.channel.owner.playerID.steamID;
            Dictionary<ushort, Coroutine> activeCooldowns;
            if (!activeCooldownDict.TryGetValue(steamId, out activeCooldowns))
            {
                activeCooldowns = new Dictionary<ushort, Coroutine>();
            }
            if (activeCooldowns.TryGetValue(controlFlag, out Coroutine coroutine) && coroutine != null)
            {
                player.StopCoroutine(coroutine);
            }
            activeCooldowns[controlFlag] = newCoroutine;
        }
        #endregion
    }
}
