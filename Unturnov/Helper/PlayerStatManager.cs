using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeedMann.Unturnov.Helper
{
    public class PlayerStatManager
    {
        static public PlayerStats GetPlayerStats(UnturnedPlayer player)
        {
            if (player == null)
                return null;

            PlayerLife life = player.Player.life;
            PlayerStats stats = new PlayerStats();

            stats.health = life.health;
            stats.food = life.food;
            stats.water = life.water;
            stats.virus = life.virus;
            stats.brokenLegs = (byte)(life.isBroken ? 1 : 0);
            stats.bleeding = (byte)(life.isBleeding ? 1 : 0);

            return stats;
        }

        static public void SetPlayerStats(UnturnedPlayer player, PlayerStats stats)
        {
            if (player == null || stats == null)
                return;

            PlayerLife life = player.Player.life;
            
            if(life.health <= stats.health)
            {
                life.serverModifyHealth(stats.health - life.health);
            }
            else
            {
                // to damage player in safezone
                life.askDamage((byte)(life.health - stats.health), Vector3.up, EDeathCause.SUICIDE, ELimb.SPINE, CSteamID.Nil, out EPlayerKill eplayerKill, false, ERagdollEffect.NONE, false, true);
            }
            
            life.serverModifyFood(stats.food - life.food);
            life.serverModifyWater(stats.water - life.water);
            life.serverModifyVirus(stats.virus - life.virus);
            life.serverSetBleeding(stats.bleeding > 0);
            life.serverSetLegsBroken(stats.brokenLegs > 0);
            
        }
    }
}
