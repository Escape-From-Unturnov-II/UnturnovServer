using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedMann.Unturnov.Commands
{
    class ScavCommands : IRocketCommand
    {
        public string Help
        {
            get { return "scav"; }
        }

        public string Name
        {
            get { return "scav"; }
        }

        public string Syntax
        {
            get { return "<scav>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player;}
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "scav" };
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
                    case "state":
                        if (ScavRunControler.tryGetStateName(player, out string state))
                        {
                            UnturnedChat.Say(caller, $"ScavRun state: {state}", UnityEngine.Color.green);
                        }
                        break;
                    case "start":
                        if (isInSafezone(player) || player.IsAdmin)
                        {
                            if (!caller.HasPermission("scav.start"))
                            {
                                UnturnedChat.Say(caller, "You are not allowed to use this command", UnityEngine.Color.red);
                                throw new WrongUsageOfCommandException(caller, this);
                            }
                            if (!ScavRunControler.tryStartScavRun(player))
                            {
                                UnturnedChat.Say(caller, "You are already a scav", UnityEngine.Color.red);
                            }

                        }
                        break;
                    case "stop":
                        if (isInSafezone(player) || player.IsAdmin)
                        {
                            if (!ScavRunControler.tryStopScavRun(player))
                            {
                                UnturnedChat.Say(caller, "You are not a scav", UnityEngine.Color.red);
                            }
                        }
                        break;
                    default:
                        UnturnedChat.Say(caller, "Invalid Command parameters", UnityEngine.Color.red);
                        throw new WrongUsageOfCommandException(caller, this);
                }
            }

        }

       public bool isInSafezone(UnturnedPlayer player, bool requiresSafezone = true)
       {
            if (!player.Player.movement.isSafeInfo?.noWeapons ?? true)
            {
                if (requiresSafezone)
                {
                    UnturnedChat.Say(player, "This command can only be used in safezone", UnityEngine.Color.red);
                }
                return false;
            }
            if (!requiresSafezone)
            {
                UnturnedChat.Say(player, "This command can not be used in safezone", UnityEngine.Color.red);
            }
            return true;
       }

        public static string formatTime(float timeInSec)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSec);

            uint d = (uint)timeSpan.Days;
            uint h = (uint)timeSpan.Hours;
            uint m = (uint)timeSpan.Minutes;
            uint s = (uint)timeSpan.Seconds;
            return (d > 0 ? $"{d} days ":"")
                + (h > 0 ? $"{h} hours " : "")
                + (m > 0 ? $"{m} minutes " : "")
                + (s > 0 ? $"{s} seconds " : "");
        }
    }
}
