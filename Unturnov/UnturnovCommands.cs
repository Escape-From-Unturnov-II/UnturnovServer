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

namespace SpeedMann.Unturnov
{
    class UnturnovCommands : IRocketCommand
    {
        public string Help
        {
            get { return "unturnov"; }
        }

        public string Name
        {
            get { return "unturnov"; }
        }

        public string Syntax
        {
            get { return "<unturnov>"; }
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
                return new List<string>() { "unturnov" };
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
                        if(player.Player.equipment?.asset != null && player.Player.equipment?.asset is ItemGunAsset)
                        {
                            ItemGunAsset asset = (ItemGunAsset)player.Player.equipment?.asset;
                            GunAttachments attachments = new GunAttachments(player.Player.equipment.state);

                            UnturnedChat.Say($"Stats of equiped Item are: sight { attachments.attachments[0].id}, tactical { attachments.attachments[1].id}, grip { attachments.attachments[2].id}, barrel { attachments.attachments[3].id}, mag { attachments.magAttachment.id}, ammo { attachments.ammo }, ");
                        }
                        else
                        {
                            UnturnedChat.Say($"No gun equiped");
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
