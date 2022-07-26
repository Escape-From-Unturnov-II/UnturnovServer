using Rocket.Core.Logging;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    internal class SetFalgQueueEntry : MainQueueEntry
    {
        internal PlayerQuests quests;
        internal ushort flagId;
        internal short value;

        internal SetFalgQueueEntry(PlayerQuests quests, ushort flagId, short value)
        {
            this.quests = quests;
            this.flagId = flagId;
            this.value = value;
        }
        public void Run()
        {
            Logger.Log($"ScavRun for {quests.player.name} is off cooldown");
            quests.sendSetFlag(flagId, value);

        }
    }
}
