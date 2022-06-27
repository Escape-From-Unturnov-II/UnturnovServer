using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            life.ReceiveHealth(stats.health);
            life.ReceiveFood(stats.food);
            life.ReceiveWater(stats.water);
            life.ReceiveVirus(stats.virus);
            life.ReceiveBroken(stats.brokenLegs > 0);
            life.ReceiveBleeding(stats.bleeding > 0);
        }
    }
}
