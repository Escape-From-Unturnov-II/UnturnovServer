using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Helper
{
    public class TeleportControler
    {
        internal static void OnFlagChanged(PlayerQuests quests, PlayerQuestFlag flag)
        {
            if (flag.id == Unturnov.Conf.ScavRunControlFlag)
            {
                UnturnedPlayer player = UnturnedPlayer.FromPlayer(quests.player);
            }
        }

        internal void randomlyTeleportPlayer(UnturnedPlayer player)
        {

        }
        internal void randomlyTeleportSquad(UnturnedPlayer player)
        {
            //TODO: implement
        }
    }
}
