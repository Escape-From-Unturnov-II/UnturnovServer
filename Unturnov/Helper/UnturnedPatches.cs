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
        private static string harmonyId = "SpeedMann.Unturnov";
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
                Logger.LogError($"Unturnov patches: {e.Message}");
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
        public delegate void PreTryAddItemAuto(PlayerInventory inventory, Item item, ref bool autoEquipWeapon, ref bool autoEquipUseable, ref bool autoEquipClothing);
        public static event PreTryAddItemAuto OnPreTryAddItemAuto;
        #endregion

        #region Patches

        [HarmonyPatch(typeof(PlayerInventory), nameof(PlayerInventory.tryAddItemAuto), new Type[] { typeof(Item), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        class TryAddItemAutoPatch
        {
            [HarmonyPrefix]
            internal static bool OnPreTryAddItemAutoInvoker(PlayerInventory __instance, Item item, ref bool autoEquipWeapon, ref bool autoEquipUseable, ref bool autoEquipClothing)
            {
                OnPreTryAddItemAuto?.Invoke(__instance, item, ref autoEquipWeapon, ref autoEquipUseable, ref autoEquipClothing);
                return true;
            }
        }
              
        #endregion
    }
}
