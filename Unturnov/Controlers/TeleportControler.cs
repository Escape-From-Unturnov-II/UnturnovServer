using Rocket.Unturned.Player;
using SDG.Framework.Devkit;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Controlers
{
    public class TeleportControler
    {
        private static TeleportConfig Conf;
        private static Dictionary<ushort, RaidTeleport> teleportConfigDict;
        
        internal static void Init(TeleportConfig teleportConfig)
        {
            Conf = teleportConfig;
            teleportConfigDict = createDictionaryOfTeleportConfigs(teleportConfig.RaidTeleports);
        }
        internal static void Cleanup()
        {
            teleportConfigDict.Clear();
        }

        internal static void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(quests.player);
            if(!teleportConfigDict.TryGetValue(flag.id, out RaidTeleport teleport))
            {
                return;
            }

            TeleportDestination dest;
            switch (flag.value)
            {
                case -1:
                    dest = getRandomTeleportLocation(teleport);
                    teleportPlayer(player, dest);
                    break;
                case -2:
                    dest = getRandomTeleportLocation(teleport);
                    teleportSquad(player, dest);
                    break;
            }
        }

        internal static void teleportPlayer(UnturnedPlayer player, TeleportDestination destination)
        {
            destination.findDestination(out Vector3 position, out float rotation);
            
            player.Teleport(position, rotation != 0 ? rotation : MeasurementTool.angleToByte(player.Rotation));

            Logger.Log($"{player.DisplayName} was teleported to {destination.NodeName} [{position.x}, {position.y}, {position.z}]");
        }
        internal static void teleportSquad(UnturnedPlayer caller, TeleportDestination destination)
        {
            foreach (SteamPlayer client in Provider.clients)
            {
                if (!(client.player == null) && client.playerID.steamID != caller.CSteamID && client.player.quests.isMemberOfGroup(caller.SteamGroupID))
                {
                    if (Vector3.SqrMagnitude(client.player.transform.position - caller.Position) < Conf.SquadTeleportRadius * Conf.SquadTeleportRadius)
                    {
                        teleportPlayer(UnturnedPlayer.FromCSteamID(client.playerID.steamID), destination);
                    }
                }
            }
            teleportPlayer(caller, destination);
        }

        internal static TeleportDestination getRandomTeleportLocation(RaidTeleport config)
        {
            if(config.TeleportDestinations.Count > 0)
            {
                return config.TeleportDestinations[UnityEngine.Random.Range(0, config.TeleportDestinations.Count)];
            }
            return null;
        }
        internal static Dictionary<ushort, RaidTeleport> createDictionaryOfTeleportConfigs(List<RaidTeleport> teleports)
        {
            var teleportDict = new Dictionary<ushort, RaidTeleport>();
            if (teleports != null)
            {
                foreach (var teleport in teleports)
                {
                    if (teleport.TeleportFlag == 0)
                    {
                        Logger.LogWarning("TeleportConfig with TeleportFlag 0 is invalid");
                        continue;
                    }

                    if (teleportDict.ContainsKey(teleport.TeleportFlag))
                    {
                        Logger.LogError($"TeleportConfig with TeleportFlag:{teleport.TeleportFlag} is a duplicate!");
                        continue;
                    }
                    teleportDict.Add(teleport.TeleportFlag, teleport);
                }
            }
            return teleportDict;
        }
    }
}
