using HarmonyLib;
using Rocket.Core.Logging;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Helper
{
    class UnturnedPatches
    {
        private static Harmony harmony;
        private static string harmonyId = "SpeedMann.PvPRework";
        public static void Init()
        {
            try
            {
                harmony = new Harmony(harmonyId);
                harmony.PatchAll();
                if (Unturnov.Conf.Debug)
                {
                    var myOriginalMethods = harmony.GetPatchedMethods();
                    Logger.Log("Patched Methods:");
                    foreach (var method in myOriginalMethods)
                    {
                        Logger.Log(" " + method.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"ArmorPlus patches: {e.Message}");
            }
        }
        public static void Cleanup()
        {
            try
            {
                harmony.UnpatchAll(harmonyId);

                if (Unturnov.Conf.Debug)
                {
                    var myOriginalMethods = harmony.GetPatchedMethods();
                    Logger.Log("Patched Methods:");
                    foreach (var method in myOriginalMethods)
                    {
                        Logger.Log(" " + method.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"ArmorPlus patches: {e.Message}");
            }
        }

        #region Events
        public delegate void PrePlayerDraggedItem(PlayerInventory inventory, byte page_0, byte x_0, byte y_0,
           byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow);
        public static event PrePlayerDraggedItem OnPrePlayerDraggedItem;
        public delegate void PrePlayerSwappedItem(PlayerInventory inventory, byte page_0, byte x_0, byte y_0,
           byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow);
        public static event PrePlayerSwappedItem OnPrePlayerSwappedItem;
        #endregion

        #region Patches

        [HarmonyPatch(typeof(Provider), nameof(Provider.accept), new Type[] { typeof(SteamPending) })]
        class ClientAcceptedPatch
        {
            [HarmonyPrefix]
            internal static bool OnPreClientAcceptedInvoker(PlayerInventory __instance, SteamPending player)
            {
                Logger.Log($"Client Backpack cosmetic: {player.backpackItem}");
                player.backpackItem = 83000;
                
                return true;
            }
        }
              
        #endregion
    }
}
