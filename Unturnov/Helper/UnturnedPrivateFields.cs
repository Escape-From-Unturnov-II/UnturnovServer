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
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    public class UnturnedPrivateFields
    {   
        private static FieldInfo ItemJarItemInfo;
        private static MethodInfo ProviderWriteConnectedMessageInfo;
        private static MethodInfo ProviderBroadcastConnectInfo;
        private static MethodInfo ProviderBroadcastDisconnectInfo;

        private static FieldInfo PlayerLifeLastTimeDamagedInfo;
        private static FieldInfo PlayerLifeRecentKillerInfo;
        private static FieldInfo PlayerLifeConbatCooldownInfo;

        private static FieldInfo UseableBarricadeIsUsingInfo;
        private static FieldInfo UseableBarricadeStartedUseInfo;
        private static FieldInfo UseableBarricadeUseTimeInfo;

        private static FieldInfo BarricadeDropServersideDataInfo;

        private static FieldInfo UseableGunAmmoInfo;

        public static bool TryBroadcastConnect(SteamPlayer player)
        {
            if (ProviderBroadcastConnectInfo != null)
            {
                ProviderBroadcastConnectInfo.Invoke(null, new object[] { player });
                return true;
            }
            return false;
        }
        public static bool TryBroadcastDisconnect(SteamPlayer player)
        {
            if (ProviderBroadcastDisconnectInfo != null)
            {
                ProviderBroadcastDisconnectInfo.Invoke(null, new object[] { player });
                return true;
            }
            return false;
        }
        public static bool WriteConnectedMessage(NetPakWriter writer, SteamPlayer aboutPlayer, SteamPlayer forPlayer)
        {
            if (ProviderWriteConnectedMessageInfo != null)
            {
                ProviderWriteConnectedMessageInfo.Invoke(null, new object[] { writer, aboutPlayer, forPlayer });
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

            if (PlayerLifeLastTimeDamagedInfo != null)
            {
                try
                {
                    result = (float)PlayerLifeLastTimeDamagedInfo.GetValue(playerLife);
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

            if (PlayerLifeRecentKillerInfo != null)
            {
                try
                {
                    result = (CSteamID)PlayerLifeRecentKillerInfo.GetValue(playerLife);
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

            if (PlayerLifeConbatCooldownInfo != null)
            {
                try
                {
                    result = (float)PlayerLifeConbatCooldownInfo.GetValue(playerLife);
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

            if (UseableBarricadeIsUsingInfo != null && useableBarricade != null)
            {
                try
                {
                    result = (bool)UseableBarricadeIsUsingInfo.GetValue(useableBarricade);
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

            if (UseableBarricadeStartedUseInfo != null && UseableBarricadeUseTimeInfo != null && useableBarricade != null)
            {
                try
                {
                    float startedUse = (float)UseableBarricadeStartedUseInfo.GetValue(useableBarricade);
                    float useTime = (float)UseableBarricadeUseTimeInfo.GetValue(useableBarricade);

                    result = Time.realtimeSinceStartup - startedUse > useTime;
                    
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "Exception loading private field StartedUseInfo or UseTimeInfo");
                    return false;
                }
            }
            return result;
        }

        public static bool TryGetServersideData(BarricadeDrop barricadeDrop, out BarricadeData serversideData)
        {
            serversideData = null;

            if (BarricadeDropServersideDataInfo != null && barricadeDrop != null)
            {
                try
                {
                    serversideData = (BarricadeData)BarricadeDropServersideDataInfo.GetValue(barricadeDrop);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "Exception loading private field ServersideDataInfo");
                    return false;
                }
            }
            return false;
        }

        public static bool TryGetAmmo(UseableGun gun, out byte ammo)
        {
            ammo = 0;

            if (UseableGunAmmoInfo != null && gun != null)
            {
                try
                {
                    ammo = (byte)UseableGunAmmoInfo.GetValue(gun);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "Exception loading private field UseableGunAmmoInfo");
                    return false;
                }
            }
            return false;
        }

        public static void Init()
        {
            Type type;

            type = typeof(Provider);
            ProviderWriteConnectedMessageInfo = type.GetMethod("WriteConnectedMessage", BindingFlags.Static | BindingFlags.NonPublic);
            ProviderBroadcastConnectInfo = type.GetMethod("broadcastEnemyConnected", BindingFlags.Static | BindingFlags.NonPublic);
            ProviderBroadcastDisconnectInfo = type.GetMethod("broadcastEnemyDisconnected", BindingFlags.Static | BindingFlags.NonPublic);

            type = typeof(ItemJar);
            ItemJarItemInfo = type.GetField("_item", BindingFlags.NonPublic);

            type = typeof(PlayerLife);
            PlayerLifeLastTimeDamagedInfo = type.GetField("lastTimeTookDamage", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerLifeRecentKillerInfo = type.GetField("recentKiller", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerLifeConbatCooldownInfo = type.GetField("COMBAT_COOLDOWN", BindingFlags.NonPublic | BindingFlags.Static);

            type = typeof(UseableBarricade);
            UseableBarricadeIsUsingInfo = type.GetField("isUsing", BindingFlags.NonPublic | BindingFlags.Instance);
            UseableBarricadeStartedUseInfo = type.GetField("startedUse", BindingFlags.NonPublic | BindingFlags.Instance);
            UseableBarricadeUseTimeInfo = type.GetField("useTime", BindingFlags.NonPublic | BindingFlags.Instance);

            type = typeof(BarricadeDrop);
            BarricadeDropServersideDataInfo = type.GetField("serversideData", BindingFlags.NonPublic | BindingFlags.Instance);

            type = typeof(UseableGun);
            UseableGunAmmoInfo = type.GetField("ammo", BindingFlags.NonPublic | BindingFlags.Instance);
        /*
        type = AccessTools.TypeByName("SDG.Unturned.NetMessages");
        WriterInfo = AccessTools.Field(type, "writer");
        */
    }
    }
}
