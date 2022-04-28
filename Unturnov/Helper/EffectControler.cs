using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Logging;

namespace SpeedMann.Unturnov.Helper
{
    public class EffectControler
    {
        public static void spawnUI(ushort effectId, short effectKey, CSteamID executorID)
        {
            ITransportConnection transportConnection = Provider.findTransportConnection(executorID);
            if (transportConnection == null)
            {
                Logger.LogError("Error in SecureCase UI while trying to show UI (CSteamID not found)");
                return;
            }
            EffectManager.sendUIEffect(effectId, effectKey, transportConnection, true);
        }
    }
}
