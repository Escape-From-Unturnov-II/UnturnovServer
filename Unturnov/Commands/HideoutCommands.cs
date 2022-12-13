using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Controlers;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    case "getpos":
                        Vector3 playerRotation = player.Player.transform.eulerAngles;
                        string pos = $"Your position: x: {player.Position.x}, y: {player.Position.y}, z: {player.Position.z} rotation: {playerRotation}";
                        UnturnedChat.Say(caller, pos, Color.cyan);
                        Logger.Log(pos);
                        break;
                    case "change":
                        EffectControler.hideBorders(player.CSteamID);

                        HideoutControler.freeHideout(player);
                        HideoutControler.claimHideout(player);
                        Hideout hideout = HideoutControler.getHideout(player.CSteamID);

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
                                Vector3[] points = new Vector3[4]
                                {
                                    hideout.bounds[0],
                                    new Vector3(hideout.bounds[0].x, hideout.bounds[0].y, hideout.bounds[1].z),
                                    hideout.bounds[1],
                                    new Vector3(hideout.bounds[1].x, hideout.bounds[0].y, hideout.bounds[0].z),
                                };
                                EffectControler.spawnBorder(player.CSteamID, points[0], points[1], hideout.bounds[0].y, hideout.bounds[1].y);
                                EffectControler.spawnBorder(player.CSteamID, points[1], points[2], hideout.bounds[0].y, hideout.bounds[1].y);
                                EffectControler.spawnBorder(player.CSteamID, points[2], points[3], hideout.bounds[0].y, hideout.bounds[1].y);
                                EffectControler.spawnBorder(player.CSteamID, points[3], points[0], hideout.bounds[0].y, hideout.bounds[1].y);
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
    }
}
