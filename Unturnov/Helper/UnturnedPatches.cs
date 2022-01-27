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
        public static void Init()
        {
            try
            {
                Harmony harmony = new Harmony("SpeedMann.Unturnov");
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
                Logger.LogError($"EventLoad: {e.Message}");
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


        [HarmonyPatch(typeof(PlayerInventory), "ReceiveDragItem")]
        class InventoryDrag
        {
            [HarmonyPrefix]
            internal static bool OnPreItemDraggedInvoker(PlayerInventory __instance, byte page_0, byte x_0, byte y_0,
       ref byte page_1, ref byte x_1, ref byte y_1, ref byte rot_1)
            {
                var shouldAllow = true;
                OnPrePlayerDraggedItem?.Invoke(__instance, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
                return shouldAllow;
            }
        }


        [HarmonyPatch(typeof(PlayerInventory), "ReceiveSwapItem")]
        class InventoryMove
        {
            [HarmonyPrefix]
            internal static bool OnPreItemSwappedInvoker(PlayerInventory __instance, byte page_0, byte x_0, byte y_0,
    byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1)
            {
                var shouldAllow = true;
                OnPrePlayerSwappedItem?.Invoke(__instance, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1,
                    ref shouldAllow);
                return shouldAllow;
            }
        }
        #endregion
    }
}
