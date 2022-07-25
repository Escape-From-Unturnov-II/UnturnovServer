using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Classes
{
    internal class QuestExtensionControler
    {
        private static Dictionary<CSteamID, Zombie> lastDamagedZombie;
        private static Dictionary<CSteamID, Animal> lastDamagedAnimal;

        internal static void Init()
        {
            lastDamagedZombie = new Dictionary<CSteamID, Zombie>();
            lastDamagedAnimal = new Dictionary<CSteamID, Animal>();
        }
        internal static void Cleanup()
        {
            lastDamagedZombie.Clear();
            lastDamagedAnimal.Clear();
        }
        internal static void OnPlayerDisconected(UnturnedPlayer player)
        {
            lastDamagedAnimal.Remove(player.CSteamID);
            lastDamagedZombie.Remove(player.CSteamID);
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

            if (lastDamagedZombie.ContainsKey(player.CSteamID))
            {
                lastDamagedZombie[player.CSteamID] = parameters.zombie;
                return;
            }
            lastDamagedZombie.Add(player.CSteamID, parameters.zombie);
        }
        internal static void OnZombieDeath(Zombie zombie)
        {
            UnturnedPlayer player = null;
            foreach (KeyValuePair<CSteamID, Zombie> entry in lastDamagedZombie)
            {
                if (entry.Value.Equals(zombie))
                {
                    player = UnturnedPlayer.FromCSteamID(entry.Key);
                    lastDamagedZombie.Remove(entry.Key);
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

            if (lastDamagedAnimal.ContainsKey(player.CSteamID))
            {
                lastDamagedAnimal[player.CSteamID] = parameters.animal;
                return;
            }
            lastDamagedAnimal.Add(player.CSteamID, parameters.animal);
        }
        internal static void OnAnimalDeath(Animal animal)
        {
            UnturnedPlayer player = null;
            foreach (KeyValuePair<CSteamID, Animal> entry in lastDamagedAnimal)
            {
                if (entry.Value.Equals(animal))
                {
                    player = UnturnedPlayer.FromCSteamID(entry.Key);
                    lastDamagedAnimal.Remove(entry.Key);
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
                || !UnturnedPrivateFields.TryGetCombatCooldown(player.life, out float combatCooldown))
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

        public static bool falgIdCheck()
        {
            // TODO: implement quest extension falg id check
            throw new NotImplementedException();
        }
        #endregion
    }
}
