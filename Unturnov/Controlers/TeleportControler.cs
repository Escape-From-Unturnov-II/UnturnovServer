using Google.Protobuf.WellKnownTypes;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Commands;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using SpeedMann.Unturnov.Models.Hideout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Controlers
{
    /*
        Type Dialogue
        ID [DialogueId]

        Messages 1
        Message_0_Pages 1

        Responses 2

        Response_0_Dialogue [TeleportDialogueId]
        Response_0_Conditions 1
        Response_0_Condition_0_Type Flag_Short
        Response_0_Condition_0_ID [TeleportControlFalgIdOfDestination]
        Response_0_Condition_0_Value 0
        Response_0_Condition_0_Logic Equal
        Response_0_Condition_0_Allow_Unset

        Response_1_Dialogue [DialogueId] //to return to this dialogue when giving the quest
        Response_1_Quest [QuestIdOfMap]
        Response_1_Conditions 1
        Response_1_Condition_0_Type Flag_Short
        Response_1_Condition_0_ID [TeleportControlFalgIdOfDestination]
        Response_1_Condition_0_Value 0
        Response_1_Condition_0_Logic Greater_Than
        
        //give quest
        Response_1_Rewards 1
        Response_1_Reward_0_Type Quest
        Response_1_Reward_0_ID [QuestIdOfMap]

        Type Quest
        ID [QuestIdOfMap]

        //one flag for min, one for sec
        Conditions 2
        Condition_0_Type Flag_Short
        Condition_0_ID [TimerMinutesFlagId] 
        Condition_0_Value 0
        Condition_0_Logic Equal_To
        Condition_0_Reset

        Condition_1_Type Flag_Short
        Condition_1_ID [TimerSecondsFlagId]
        Condition_1_Value 0
        Condition_1_Logic Equal_To
        Condition_1_Reset

        Name <color=rare>Map Cooldown</color>
        Description You recently died on [MapName] and need to wait before entering it again!

        Condition_0 Wait {0} minutes
        Condition_1 and {0} seconds
     */
    public class TeleportControler
    {
        private static TeleportConfig Conf;
        private static Dictionary<ushort, RaidTeleport> teleportConfigDict;

        private const short teleportReady = 0;
        private const short inRaid = -1;
        private const short teleportSolo = -2;
        private const short teleportSquad = -3;
        private const short teleportBack = -4;
        private const short onCooldown = -5;
        internal static void Init(TeleportConfig teleportConfig)
        {
            Conf = teleportConfig;
            teleportConfigDict = createDictionaryOfTeleportConfigs(teleportConfig.RaidTeleports);

            foreach (SteamPlayer player in Provider.clients)
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player.player);
                OnPlayerConnected(uPlayer);
            }
        }
        internal static void Cleanup()
        {
            teleportConfigDict.Clear();
        }
        internal static void OnPlayerConnected(UnturnedPlayer player)
        {
            bool isInRaid = false;
            foreach (var teleportFlagEntries in teleportConfigDict)
            {
                if (player.Player.quests.getFlag(teleportFlagEntries.Key, out short flagValue))
                {
                    switch (flagValue)
                    {
                        case teleportReady:
                            break;
                        case inRaid:
                            isInRaid = true;
                            break;
                        case onCooldown:
                            TryStartMapCooldown(player.Player, teleportFlagEntries.Value, true);
                            break;
                        default:
                            Logger.LogError($"Player {player.CSteamID} joined with invalid teleport flag {teleportFlagEntries.Key} value {flagValue}");
                            player.Player.quests.sendSetFlag(teleportFlagEntries.Key, teleportReady);
                            break;
                    }
                }
            }

            if (!isInRaid)
            {
                TeleportToHideoutOrSpawn(player);
            }
        }
        internal static void OnPreDisconnectSave(UnturnedPlayer player)
        {
        }
        internal static void OnPlayerRevived(PlayerLife playerLife)
        {
            foreach (var teleportEntry in teleportConfigDict)
            {
                if (playerLife.player.quests.getFlag(teleportEntry.Key, out short flagValue))
                {
                    if (flagValue == inRaid)
                    {
                        UnturnedPlayer player = UnturnedPlayer.FromPlayer(playerLife.player);
                        UnturnedChat.Say(player, Util.Translate("raid_cooldown", teleportEntry.Value.RaidName, ScavCommands.formatTime(teleportEntry.Value.CooldownInMin * 60)), Color.green);
                        TryStartMapCooldown(playerLife.player, teleportEntry.Value);
                        return;
                    }
                }
            }
        }
        internal static void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(quests.player);
            if (flag.id == Conf.HideoutTeleportFlag)
            {
                CheckHideoutTeleports(player, flag.value);
                return;
            }

            if (teleportConfigDict.TryGetValue(flag.id, out RaidTeleport teleport))
            {
                CheckRaidTeleports(player, flag, teleport);
                return;
            }
        }
        internal static bool TryTeleportToHideout(UnturnedPlayer player)
        {
            Hideout hideout = HideoutControler.getHideout(player.CSteamID);
            if (hideout == null)
            {
                return false;
            }

            Vector3 point = new Vector3(hideout.originPosition.x, hideout.originPosition.y + 0.5f, hideout.originPosition.z);
            float rotation = hideout.originRotationEuler.x + HideoutControler.GetHideoutSpawnRotation();
            player.Teleport(point, rotation);
            Logger.Log($"{player.DisplayName} was teleported to his hideout at ({point})");
            return true;
        }
        internal static bool TryTeleportToBed(UnturnedPlayer player)
        {
            if (BarricadeManager.tryGetBed(player.CSteamID, out var point, out var angle))
            {
                point.y += 0.5f;
                player.Teleport(point, angle);

                return true;
            }
            return false;
        }


        private static void CheckRaidTeleports(UnturnedPlayer player, PlayerQuestFlag flag, RaidTeleport teleport)
        {
            
            switch (flag.value)
            {
                case teleportReady:
                    UnturnedChat.Say(player, Util.Translate("scav_ready", teleport.RaidName), Color.green);
                    break;
                case inRaid:
                    break;
                case teleportSolo:
                    EnterRaid(player, teleport, false);
                    break;
                case teleportSquad:
                    EnterRaid(player, teleport, true);
                    break;
                case teleportBack:
                    TeleportPlayer(player, teleport.ExtractDestination, flag.id, false);
                    break;
                default:
                    Logger.LogError($"Player {player.CSteamID} got invalid teleport flag {flag.id} value {flag.value}");
                    break;
            }
        }
        private static void CheckHideoutTeleports(UnturnedPlayer player, short flagValue)
        {
            switch (flagValue)
            {
                case teleportSolo:
                    if (!TryTeleportToHideout(player))
                    {
                        Logger.LogWarning($"Could not teleport {player.CSteamID} to hideout!");
                    }
                    Unturnov.ChangeFlagDelayed(player.Player, Conf.HideoutTeleportFlag, teleportReady);
                    break;
            }
        }
        private static void EnterRaid(UnturnedPlayer player, RaidTeleport teleport, bool withSquad)
        {
            TeleportDestination dest = getRandomTeleportLocation(teleport);
            if (withSquad)
            {
                TeleportSquad(player, dest, teleport.TeleportFlag);
            }
            else
            {
                TeleportPlayer(player, dest, teleport.TeleportFlag);
            }
        }
        private static void TeleportPlayer(UnturnedPlayer player, TeleportDestination destination, ushort flagId, bool enterRaid = true)
        {
            destination.findDestination(out Vector3 position, out float rotation);

            player.Teleport(position, rotation != 0 ? rotation : MeasurementTool.angleToByte(player.Rotation));
            if (!enterRaid)
            {
                Unturnov.ChangeFlagDelayed(player.Player, flagId, teleportReady);
            }
            else
            {
                Unturnov.ChangeFlagDelayed(player.Player, flagId, inRaid);
            }

            Logger.Log($"{player.DisplayName} was teleported to {destination.NodeName} [{position.x}, {position.y}, {position.z}]");
        }
        private static void TeleportSquad(UnturnedPlayer caller, TeleportDestination destination, ushort flagId)
        {
            foreach (SteamPlayer client in Provider.clients)
            {
                if (!(client.player == null) && client.playerID.steamID != caller.CSteamID && client.player.quests.isMemberOfGroup(caller.SteamGroupID))
                {
                    if (Vector3.SqrMagnitude(client.player.transform.position - caller.Position) < Conf.SquadTeleportRadius * Conf.SquadTeleportRadius)
                    {
                        TeleportPlayer(UnturnedPlayer.FromCSteamID(client.playerID.steamID), destination, flagId);
                    }
                }
            }
            TeleportPlayer(caller, destination, flagId);
        }
        private static void TeleportToHideoutOrSpawn(UnturnedPlayer player)
        {
            if (!TryTeleportToHideout(player))
            {
                TeleportToSpawn(player);
            }
        }
        private static void TeleportToBedOrSpawn(UnturnedPlayer player)
        {
            if (!TryTeleportToBed(player))
            {
                TeleportToSpawn(player);
            }
            
        }
        private static void TeleportToSpawn(UnturnedPlayer player)
        {
            Vector3 point = player.Player.transform.position;
            float angle = 0;
            var spawnpoint = LevelPlayers.getSpawn(isAlt: false);
            if (spawnpoint != null)
            {
                point = spawnpoint.point;
                angle = MeasurementTool.angleToByte(spawnpoint.angle);
            }
            point.y += 0.5f;
            player.Teleport(point, angle);
        }
        private static TeleportDestination getRandomTeleportLocation(RaidTeleport config)
        {
            if(config.TeleportDestinations.Count > 0)
            {
                return config.TeleportDestinations[UnityEngine.Random.Range(0, config.TeleportDestinations.Count)];
            }
            return null;
        }
        private static Dictionary<ushort, RaidTeleport> createDictionaryOfTeleportConfigs(List<RaidTeleport> teleports)
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
        private static void TryStartMapCooldown(Player player, RaidTeleport raidTeleport, bool restore = false)
        {
            player.StartCoroutine(TryStartMapCooldownDelayed(player, raidTeleport, restore));
        }
        private static IEnumerator TryStartMapCooldownDelayed(Player player, RaidTeleport raidTeleport, bool restore = false)
        {
            yield return null;
            QuestExtensionControler.TryStartQuestBasedCooldown(player,
                raidTeleport.QuestCooldown,
                raidTeleport.CooldownInMin,
                raidTeleport.TeleportFlag,
                onCooldown,
                teleportReady,
                restore);
        }
    }
}
