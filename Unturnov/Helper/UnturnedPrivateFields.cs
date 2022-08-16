using HarmonyLib;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    public class UnturnedPrivateFields
    {   
        private static FieldInfo ItemJarItemInfo;
        private static MethodInfo WriteConnectedMessageInfo;
        private static MethodInfo BroadcastConnectInfo;
        private static MethodInfo BroadcastDisconnectInfo;
        private static FieldInfo WriterInfo;

        private static FieldInfo LastTimeDamagedInfo;
        private static FieldInfo RecentKillerInfo;
        private static FieldInfo ConbatCooldownInfo;

        private static FieldInfo IsUsingInfo;
        private static FieldInfo IsUseableInfo;
        


        public static bool TryBroadcastConnect(SteamPlayer player)
        {
            if (BroadcastConnectInfo != null)
            {
                BroadcastConnectInfo.Invoke(null, new object[] { player });
                return true;
            }
            return false;
        }
        public static bool TryBroadcastDisconnect(SteamPlayer player)
        {
            if (BroadcastDisconnectInfo != null)
            {
                BroadcastDisconnectInfo.Invoke(null, new object[] { player });
                return true;
            }
            return false;
        }
        public static bool TryGetNetMessagesWriter(out NetPakWriter writer)
        {
            writer = null;
            if (WriterInfo != null)
            {
                writer = (NetPakWriter)WriterInfo.GetValue(null);
                return true;
            }
            return false;
        }
        public static bool WriteConnectedMessage(NetPakWriter writer, SteamPlayer aboutPlayer, SteamPlayer forPlayer)
        {
            if (WriteConnectedMessageInfo != null)
            {
                WriteConnectedMessageInfo.Invoke(null, new object[] { writer, aboutPlayer, forPlayer });
                return true;
            }
            return false;
        }
        public static bool TrySetItemJarItem(ItemJar itemJar, Item newItem)
        {
            if (ItemJarItemInfo != null)
            {
                ItemJarItemInfo.SetValue(itemJar, newItem);
                return true;
            }
            return false;
        }

        public static bool TryGetLastTimeDamaged(PlayerLife playerLife, out float result)
        {
            result = -100f;

            if (LastTimeDamagedInfo != null)
            {
                try
                {
                    result = (float)LastTimeDamagedInfo.GetValue(playerLife);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "Exception loading private field lastTimeDamaged");
                    return false;
                }
                return true;
            }
            return false;
        }
        public static bool TryGetRecentKiller(PlayerLife playerLife, out CSteamID result)
        {
            result = CSteamID.Nil;

            if (RecentKillerInfo != null)
            {
                try
                {
                    result = (CSteamID)RecentKillerInfo.GetValue(playerLife);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "Exception loading private field recentKiller");
                    return false;
                }
                return true;
            }
            return false;
        }
        public static bool TryGetCombatCooldown(PlayerLife playerLife, out float result)
        {
            result = 30;

            if (ConbatCooldownInfo != null)
            {
                try
                {
                    result = (float)ConbatCooldownInfo.GetValue(playerLife);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "Exception loading private field conbatCooldown");
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool TryGetIsUsing(UseableBarricade useableBarricade)
        {
            bool result = false;

            if (ConbatCooldownInfo != null)
            {
                try
                {
                    result = (bool)IsUsingInfo.GetValue(useableBarricade);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "Exception loading private field IsUsing");
                    return false;
                }
            }
            return result;
        }
        public static bool TryGetIsUseable(UseableBarricade useableBarricade)
        {
            bool result = false;

            if (ConbatCooldownInfo != null)
            {
                try
                {
                    result = (bool)IsUseableInfo.GetValue(useableBarricade);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "Exception loading private field IsUseable");
                    return false;
                }
            }
            return result;
        }

        public static void Init()
        {
            Type type;

            type = typeof(Provider);
            WriteConnectedMessageInfo = type.GetMethod("WriteConnectedMessage", BindingFlags.Static | BindingFlags.NonPublic);
            BroadcastConnectInfo = type.GetMethod("broadcastEnemyConnected", BindingFlags.Static | BindingFlags.NonPublic);
            BroadcastDisconnectInfo = type.GetMethod("broadcastEnemyDisconnected", BindingFlags.Static | BindingFlags.NonPublic);

            type = typeof(ItemJar);
            ItemJarItemInfo = type.GetField("_item", BindingFlags.NonPublic);

            type = typeof(PlayerLife);
            LastTimeDamagedInfo = type.GetField("lastTimeTookDamage", BindingFlags.NonPublic | BindingFlags.Instance);
            RecentKillerInfo = type.GetField("recentKiller", BindingFlags.NonPublic | BindingFlags.Instance);
            ConbatCooldownInfo = type.GetField("COMBAT_COOLDOWN", BindingFlags.NonPublic | BindingFlags.Static);

            type = typeof(UseableBarricade);
            IsUseableInfo = type.GetField("isUsing", BindingFlags.NonPublic | BindingFlags.Instance);
            IsUsingInfo = type.GetField("isUseable", BindingFlags.NonPublic | BindingFlags.Instance);
            /*
            type = AccessTools.TypeByName("SDG.Unturned.NetMessages");
            WriterInfo = AccessTools.Field(type, "writer");
            */
        }
    }
}
