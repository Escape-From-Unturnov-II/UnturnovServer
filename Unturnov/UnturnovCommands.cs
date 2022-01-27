using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedMann.Unturnov
{
    class UnturnovCommands : IRocketCommand
    {
        public string Help
        {
            get { return "Unturnov"; }
        }

        public string Name
        {
            get { return "Unturnov"; }
        }

        public string Syntax
        {
            get { return "<Unturnov>"; }
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
                return new List<string>() { "Unturnov" };
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
                    case "autocraft":
                        int index = 0;
                        if (command.Length == 2)
                        {
                            int.TryParse(command[1], out index);
                        }
                        if (Unturnov.Conf.AutoCraft.Count() > index)
                        {
                            Unturnov.sendAutoCraft(player, Unturnov.Conf.AutoCraft[index]);
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Invalid Command parameters", UnityEngine.Color.red);
                        }
                        
                        break;
                    default:
                        UnturnedChat.Say(caller, "Invalid Command parameters", UnityEngine.Color.red);
                        throw new WrongUsageOfCommandException(caller, this);
                }
            }
        }
    }
}
