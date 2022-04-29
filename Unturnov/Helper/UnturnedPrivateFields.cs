using HarmonyLib;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Helper
{
    public class UnturnedPrivateFields
    {   
        private static FieldInfo ItemJarItemInfo;
        private static MethodInfo WriteConnectedMessageInfo;
        private static MethodInfo BroadcastConnectInfo;
        private static MethodInfo BroadcastDisconnectInfo;
        private static FieldInfo WriterInfo;

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


        
        public static void Init()
        {
            Type type;

            type = typeof(Provider);
            WriteConnectedMessageInfo = type.GetMethod("WriteConnectedMessage", BindingFlags.Static | BindingFlags.NonPublic);
            BroadcastConnectInfo = type.GetMethod("broadcastEnemyConnected", BindingFlags.Static | BindingFlags.NonPublic);
            BroadcastDisconnectInfo = type.GetMethod("broadcastEnemyDisconnected", BindingFlags.Static | BindingFlags.NonPublic);

            type = typeof(ItemJar);
            ItemJarItemInfo = type.GetField("_item", BindingFlags.NonPublic);
            
            /*
            type = AccessTools.TypeByName("SDG.Unturned.NetMessages");
            WriterInfo = AccessTools.Field(type, "writer");
            */
        }
    }
}
