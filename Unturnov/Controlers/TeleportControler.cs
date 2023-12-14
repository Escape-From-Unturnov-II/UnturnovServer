using Rocket.Unturned.Player;
using SDG.Framework.Devkit;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using SpeedMann.Unturnov.Models.Hideout;
using System;
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
        }
        internal static void Cleanup()
        {
            teleportConfigDict.Clear();
        }
        internal static void OnPlayerConnected(UnturnedPlayer player)
        {
            bool inRaid = false;
            foreach (ushort teleportFlagIds in teleportConfigDict.Keys)
            {
                if (player.Player.quests.getFlag(teleportFlagIds, out short flagValue) && flagValue < 0)
                {
                    inRaid = true;
                    break;
                }
            }

            if (!inRaid)
            {
                TeleportToHideoutOrSpawn(player);
            }
        }
        internal static void OnPreDisconnectSave(UnturnedPlayer player)
        {
            foreach (ushort teleportFlagIds in teleportConfigDict.Keys)
            {
                if (player.Player.quests.getFlag(teleportFlagIds, out short flagValue))
                {
                    if (flagValue != 0)
                    {

                    }
                }
            }

        }
        internal static void OnPlayerRevived(PlayerLife playerLife)
        {
            foreach (var teleportEntry in teleportConfigDict)
            {
                if (playerLife.player.quests.getFlag(teleportEntry.Key, out short flagValue))
                {
                    if (flagValue == inRaid)
                    {
                        if (!TryStartMapCooldown(playerLife.player, teleportEntry.Value))
                        {
                            playerLife.player.quests.sendSetFlag(teleportEntry.Value.TeleportFlag, teleportReady);
                            return;
                        }

                        return;
                    }
                }
            }
        }
        internal static void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(quests.player);
            if(flag.id == Conf.HideoutTeleportFlag)
            {
                CheckHideoutTeleports(player, flag.value);
                return;
            }

            if(teleportConfigDict.TryGetValue(flag.id, out RaidTeleport teleport))
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
            player.Teleport(point, hideout.originRotationEuler.x);
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
            TeleportDestination dest;
            switch (flag.value)
            {
                case teleportSolo:
                    dest = getRandomTeleportLocation(teleport);
                    TeleportPlayer(player, dest, flag.id);
                    break;
                case teleportSquad:
                    dest = getRandomTeleportLocation(teleport);
                    TeleportSquad(player, dest, flag.id);
                    break;
                case teleportBack:
                    TeleportPlayer(player, teleport.ExtractDestination, flag.id, false);
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
                    player.Player.quests.sendSetFlag(Conf.HideoutTeleportFlag, teleportReady);
                    break;
            }
        }
        private static void TeleportPlayer(UnturnedPlayer player, TeleportDestination destination, ushort flagId, bool enterRaid = true)
        {
            destination.findDestination(out Vector3 position, out float rotation);
            
            player.Teleport(position, rotation != 0 ? rotation : MeasurementTool.angleToByte(player.Rotation));
            if (!enterRaid)
            {
                player.Player.quests.sendSetFlag(flagId, teleportReady);
            }
            else
            {
                player.Player.quests.sendSetFlag(flagId, inRaid);
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
        private static bool TryStartMapCooldown(Player player, RaidTeleport raidTeleport)
        {
            if (raidTeleport.CooldownInMin <= 0)
            { 
                return false; 
            }
            if(raidTeleport.CooldownMinFlag == 0 || raidTeleport.CooldownSecFlag == 0 || raidTeleport.CooldownQuestId == 0)
            {
                Logger.LogError($"Could not start cooldown quest {raidTeleport.CooldownQuestId} for teleport {raidTeleport.TeleportFlag}!");
                return false;
            }
            QuestAsset questAsset = Assets.find(EAssetType.NPC, raidTeleport.CooldownQuestId) as QuestAsset;
            if (questAsset == null)
            {
                Logger.LogError($"Could not find cooldown quest {raidTeleport.CooldownQuestId} for teleport {raidTeleport.TeleportFlag}!");
                return false;
            }
            Logger.LogError($"Started map cooldonw for {raidTeleport.TeleportFlag}, duration {raidTeleport.CooldownInMin}min");
            player.StartCoroutine(MapCooldown(player, questAsset, raidTeleport.TeleportFlag, raidTeleport.CooldownMinFlag, raidTeleport.CooldownSecFlag, raidTeleport.CooldownInMin));
            return true;
        }
        private static IEnumerator MapCooldown(Player player, QuestAsset questAsset,ushort teleportFlag, ushort minFlagId, ushort secFlagId, float cooldownInMin)
        {
            player.quests.sendSetFlag(teleportFlag, onCooldown);
            int cooldown = Mathf.RoundToInt(cooldownInMin * 60);

            UpdateCooldown(player, minFlagId, secFlagId, cooldown);
            if (player.quests.GetQuestStatus(questAsset) == ENPCQuestStatus.NONE)
            {
                player.quests.ServerAddQuest(questAsset);
            }

            for (; cooldown >= 0; cooldown--) 
            {
                UpdateCooldown(player, minFlagId, secFlagId, cooldown);
                yield return new WaitForSeconds(1);
            }
            player.quests.sendSetFlag(teleportFlag, teleportReady);
        }
        private static void UpdateCooldown(Player player, ushort minFlagId, ushort secFlagId, int cooldownInSec)
        {
            short minutes = (short)(cooldownInSec / 60);
            short seconds = (short)(cooldownInSec % 60);
            player.quests.sendSetFlag(minFlagId, minutes);
            player.quests.sendSetFlag(secFlagId, seconds);
        }
    }
}
