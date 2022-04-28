using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    public class TeleportControler
    {
        internal static void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(quests.player);

            foreach (TeleportConfig config in Unturnov.Conf.TeleportConfigs)
            {
                if (flag.id == config.TeleportFlag || flag.id == config.SquadTeleportFlag)
                {
                    TeleportDestination dest = getRandomTeleportLocation(config);
                    if(dest != null)
                    {
                        if (flag.id == config.SquadTeleportFlag)
                        {
                            teleportSquad(player, dest);
                            break;
                        }
                        teleportPlayer(player, dest);
                        break;
                    }
                }
            }
        }
            

        internal static void teleportPlayer(UnturnedPlayer player, TeleportDestination destination)
        {
            float rotation = destination.Rotation;
            Vector3 position = destination.findDestinationPosition();
            
            player.Teleport(position, rotation != 0 ? rotation : MeasurementTool.angleToByte(player.Rotation));

            Logger.Log($"{player.DisplayName} was teleported to {destination.NodeName} [{position.x}, {position.y}, {position.z}]");
        }
        internal static void teleportSquad(UnturnedPlayer player, TeleportDestination destination)
        {
            //TODO: implement
        }

        internal static TeleportDestination getRandomTeleportLocation(TeleportConfig config)
        {
            if(config.TeleportDestinations.Count > 0)
            {
                return config.TeleportDestinations[UnityEngine.Random.Range(0, config.TeleportDestinations.Count)];
            }
            return null;
        }
    }
}
