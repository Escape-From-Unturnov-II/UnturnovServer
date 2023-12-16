using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SpeedMann.Unturnov.Models
{
    public class ScavCooldownTimer
    {
        public delegate void ScavCooldownElapsedEventHandler(PlayerQuests quests);
        public ScavCooldownElapsedEventHandler Elapsed;

        public PlayerQuests Quests;
        private Timer internalTimer;
        
        public ScavCooldownTimer(ScavKitTier tier, PlayerQuests quests)
        {
            Quests = quests;
            internalTimer = new Timer(tier.CooldownInMin > 0 ? tier.CooldownInMin * 1000 : 1000);
            internalTimer.Elapsed += timerElapsed;
            internalTimer.AutoReset = false;
        }

        private void timerElapsed(object sender, ElapsedEventArgs e)
        {
            Elapsed?.Invoke(Quests);
        }
        public void Start()
        {
            internalTimer.Start();
        }

        public void Stop()
        {
            internalTimer.Stop();
        }
    }
}
