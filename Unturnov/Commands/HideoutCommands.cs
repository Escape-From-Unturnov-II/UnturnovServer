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
                UnturnedChat.Say(caller, "Invalid!", UnityEngine.Color.red);
                return;
            }
            else
            {
                switch (command[0].ToLower())
                {
                    case "getpos":
                        string pos = $"Your position: x: {player.Position.x}, y: {player.Position.y}, z: {player.Position.z}";
                        UnturnedChat.Say(caller, pos, UnityEngine.Color.cyan);
                        Logger.Log(pos);
                        break;
                    case "change":
                        HideoutControler.freeHideout(player);
                        HideoutControler.claimHideout(player);
                        Hideout hideout = HideoutControler.getHideout(player.CSteamID);

                        UnturnedChat.Say(caller, $"Changed Hideout to {hideout.bounds[0]} {hideout.bounds[1]}", UnityEngine.Color.cyan);
                        break;
                    default:
                        UnturnedChat.Say(caller, "Invalid Command parameters", UnityEngine.Color.red);
                        throw new WrongUsageOfCommandException(caller, this);
                }
            }

        }
    }
}
