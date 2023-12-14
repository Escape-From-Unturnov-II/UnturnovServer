using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using SpeedMann.Unturnov.Controlers;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Hideout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Commands
{
    class HideoutCommands : IRocketCommand
    {
        public string Help
        {
            get { return "hideout"; }
        }

        public string Name
        {
            get { return "hideout"; }
        }

        public string Syntax
        {
            get { return "<hideout>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "hideout" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, "Invalid!", Color.red);
                return;
            }
            else
            {
                switch (command[0].ToLower())
                {
                    case "test":
                        if(!UnturnedPrivateFields.TryGetSendDialogueResponce(out var SendDialogueResponceMethod))
                        {
                            UnturnedChat.Say(caller, $"Could not open dialogue!!", Color.red);
                            break;
                        }
                        SendDialogueResponceMethod.Invoke(player.Player.GetNetId(), ENetReliability.Reliable, new Guid("abc2bb7eacc849e59eb486557221fb7f"), 0);
                        UnturnedChat.Say(caller, $"opened dialogue!", Color.cyan);
                        break;
                    case "getpos":
                        Vector3 playerRotation = player.Player.transform.eulerAngles;
                        string pos = $"Your position: x: {player.Position.x}, y: {player.Position.y}, z: {player.Position.z} rotation: {playerRotation}";
                        UnturnedChat.Say(caller, pos, Color.cyan);
                        Logger.Log(pos);
                        break;
                    case "getflag":
                        if(command.Length < 2)
                        {
                            UnturnedChat.Say(caller, $"Flag check requires flag id!", Color.red);
                            return;
                        }
                        if(!ushort.TryParse(command[1], out ushort falgId))
                        {
                            UnturnedChat.Say(caller, $"Flag id {command[1]} is invalid!", Color.red);
                            return;
                        }
                        player.Player.quests.getFlag(falgId, out short flagValue);
                        UnturnedChat.Say(caller, $"Your flag {falgId} has the value {flagValue}", Color.cyan);
                        break;
                    case "tp":
                        Hideout hideout = HideoutControler.getHideout(player.CSteamID);
                        if (hideout == null)
                        {
                            UnturnedChat.Say(caller, $"you have no hideout!", Color.red);
                            break;
                        }
                        TeleportControler.TryTeleportToHideout(player);

                        break;
                    case "change":
                        EffectControler.hideBorders(player.CSteamID);

                        HideoutControler.freeHideout(player);
                        HideoutControler.claimHideout(player);
                        hideout = HideoutControler.getHideout(player.CSteamID);
                        if(hideout == null)
                        {
                            UnturnedChat.Say(caller, $"Could not find new Hideout!", Color.red);
                            break;
                        }
                        EffectControler.spawnBorders(player.CSteamID, hideout);

                        UnturnedChat.Say(caller, $"Changed Hideout to {hideout.bounds[0]} {hideout.bounds[1]}", Color.cyan);
                        break;
                    case "border":
                        if (command.Length < 2)
                        {
                            UnturnedChat.Say(caller, $"border need a parameter. Try border <show|hide>", Color.red);
                            break;
                        }
                        string param = command[1].ToLower();
                        switch (param)
                        {
                            case "show":
                                EffectControler.hideBorders(player.CSteamID);

                                hideout = HideoutControler.getHideout(player.CSteamID);
                                if (hideout == null)
                                {
                                    UnturnedChat.Say(caller, $"you have no hideout!", Color.red);
                                }
                                EffectControler.spawnBorders(player.CSteamID, hideout);
                                break;
                            case "hide":
                                EffectControler.hideBorders(player.CSteamID);
                                break;
                            default:
                                UnturnedChat.Say(caller, $"border {param} is invalid. Try border <show|hide>", Color.red);
                                break;
                        }
                        break;
                    default:
                        UnturnedChat.Say(caller, "Invalid Command parameters", Color.red);
                        throw new WrongUsageOfCommandException(caller, this);
                }
            }

        }
        private void ShowBoarder()
        {

        }
    }
}
