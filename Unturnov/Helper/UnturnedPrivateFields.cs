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
        private static MethodInfo WriteConnectedMessageInfo;
        private static FieldInfo ItemJarItemInfo;

        public static bool TrySetItemJarItem(ItemJar itemJar, Item newItem)
        {
            if (ItemJarItemInfo != null)
            {
                ItemJarItemInfo.SetValue(itemJar, newItem);
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

        
        public static void Init()
        {
            Type type;

            type = typeof(Provider);
            WriteConnectedMessageInfo = type.GetMethod("WriteConnectedMessage", BindingFlags.Static | BindingFlags.NonPublic);

            type = typeof(ItemJar);
            ItemJarItemInfo = type.GetField("_item", BindingFlags.NonPublic);

            /*
            type = AccessTools.TypeByName("SDG.Unturned.NetMessages");
            WriterInfo = AccessTools.Field(type, "writer");
            */
        }
    }
}
