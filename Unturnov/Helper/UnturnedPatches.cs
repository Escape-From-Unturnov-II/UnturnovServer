using HarmonyLib;
using Rocket.Core.Logging;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Player;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
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
                Logger.LogError($"Unturnov patches: {e.Message}");
            }
        }

        #region Events
        // teleport
        //PlayerMovement: EnterTeleporterVolume EnterCollisionTeleporter
        // weight system
        //PlayerMovement: ReceivePluginGravityMultiplier ReceivePluginJumpMultiplier ReceivePluginSpeedMultiplier

        public delegate void PreTryAddItemAuto(PlayerInventory inventory, Item item, ref bool autoEquipWeapon, ref bool autoEquipUseable, ref bool autoEquipClothing);
        public static event PreTryAddItemAuto OnPreTryAddItemAuto;
        public delegate void PreAttachMagazine(UseableGun gun, byte page, byte x, byte y, byte[] hash);
        public static event PreAttachMagazine OnPreAttachMagazine;
        public delegate void PostAttachMagazine(UseableGun gun);
        public static event PostAttachMagazine OnPostAttachMagazine;

        public delegate void PreInteractabilityCondition(ObjectAsset objectAsset, Player player, ref bool shouldAllow);
        public static event PreInteractabilityCondition OnPreInteractabilityCondition;
        public delegate void PreItemConditionMet(NPCItemCondition itemCondition, Player player, ref bool shouldAllow);
        public static event PreItemConditionMet OnPreItemConditionMet;

        #region SecureCase
        public delegate void PrePlayerDraggedItem(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow);
        public static event PrePlayerDraggedItem OnPrePlayerDraggedItem;
        public delegate void PrePlayerSwappedItem(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow);
        public static event PrePlayerSwappedItem OnPrePlayerSwappedItem;
        public delegate void PreplayerAddItem(PlayerInventory inventory, Items page, Item item, ref bool didAdditem, ref bool shouldAllow);
        public static event PreplayerAddItem OnPrePlayerAddItem;

        public delegate void PrePlayerDead(PlayerLife playerLife);
        public static event PrePlayerDead OnPrePlayerDead;
        public delegate void PostPlayerRevive(PlayerLife playerLife);
        public static event PostPlayerRevive OnPostPlayerRevive;
        #endregion
        #region QuestExtension
        public delegate void AnimalDeath(Animal animal);  
        public static event AnimalDeath onAnimalDeath;
        public delegate void ZombieDeath(Zombie zombie);
        public static event ZombieDeath onZombieDeath;
        #endregion
        #endregion

        #region Patches
        [HarmonyPatch(typeof(UseableGun), nameof(UseableGun.ReceiveAttachMagazine), new Type[] { typeof(byte), typeof(byte), typeof(byte), typeof(byte[])})]
        class ReceiveAttachMagazinePatch
        {
            [HarmonyPrefix]
            internal static bool OnPreAttachMagazineInvoker(UseableGun __instance, byte page, byte x, byte y, byte[] hash, out UseableGun __state)
            {
                OnPreAttachMagazine?.Invoke(__instance, page, x, y, hash);
                __state = __instance;
                return true;
            }
            [HarmonyPostfix]
            internal static void OnPostAttachMagazineInvoker(UseableGun __state)
            {
                OnPostAttachMagazine?.Invoke(__state);
            }
        }

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
        [HarmonyPatch(typeof(ObjectAsset), nameof(ObjectAsset.areInteractabilityConditionsMet))]
        class InteractabilityCondition
        {
            [HarmonyPrefix]
            internal static bool OnPreInteractabilityConditionInvoker(ObjectAsset __instance, Player player, out bool __state)
            {
                var shouldAllow = true;
                OnPreInteractabilityCondition?.Invoke(__instance, player, ref shouldAllow);
                __state = shouldAllow;
                return true;
            }

            [HarmonyPostfix]
            internal static void OnPostInteractabilityConditionInvoker(ref bool __result, bool __state)
            {
                __result = __state;
            }

        }
        [HarmonyPatch(typeof(NPCItemCondition), nameof(NPCItemCondition.isConditionMet))]
        class ItemConditionMet
        {
            [HarmonyPrefix]
            internal static bool OnPreItemConditionInvoker(NPCItemCondition __instance, Player player, out bool __state)
            {
                var shouldAllow = true;
                OnPreItemConditionMet?.Invoke(__instance, player, ref shouldAllow);
                __state = shouldAllow;
                return true;
            }

            [HarmonyPostfix]
            internal static void OnPostItemConditionInvoker(ref bool __result, bool __state)
            {
                __result = __state;
            }
        }
        #region SecureCase
        internal class LiveUpdateInventory
        {
            internal PlayerInventory inventory;
            internal List<StoredItem> items;
        }


        [HarmonyPatch(typeof(PlayerLife), "ReceiveDead")]
        class PlayerDead
        {
            [HarmonyPrefix]
            internal static void OnPreLifeUpdatedInvoker(PlayerLife __instance)
            {
                OnPrePlayerDead?.Invoke(__instance);
            }


        }
        [HarmonyPatch(typeof(PlayerLife), "ReceiveRevive")]
        class PlayerRevive
        {
            [HarmonyPrefix]
            internal static void OnPreReviveInvoker(PlayerLife __instance, out PlayerLife __state)
            {
                __state = __instance;
            }
            [HarmonyPostfix]
            internal static void OnPosReviveInvoker(PlayerLife __state)
            {
                OnPostPlayerRevive?.Invoke(__state);
            }
        }

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

        [HarmonyPatch(typeof(Items), nameof(Items.tryAddItem), new Type[] { typeof(Item), typeof(bool) })]
        class PageAddItem
        {
            [HarmonyPrefix]
            internal static bool OnPreItemsAddItemInvoker(Items __instance, Item item, ref bool __result)
            {
                bool shouldAllow = true;
                object target = __instance.onStateUpdated.Target;
                if (target is PlayerInventory)
                {
                    OnPrePlayerAddItem?.Invoke((PlayerInventory)target, __instance, item, ref __result, ref shouldAllow);
                }
                __result = shouldAllow;
                return shouldAllow;
            }
        }
        #endregion
        #region QuestExtension
        [HarmonyPatch(typeof(AnimalManager), nameof(AnimalManager.sendAnimalDead))]
        class AnimalDeathManager
        {
            [HarmonyPrefix]
            internal static bool OnPreAnimalDeathInvoker(Animal animal)
            {
                onAnimalDeath?.Invoke(animal);
                return true;
            }
        }
        [HarmonyPatch(typeof(ZombieManager), nameof(ZombieManager.sendZombieDead))]
        class ZombieDeathManager
        {
            [HarmonyPrefix]
            internal static bool OnPreZombieDeathInvoker(Zombie zombie)
            {
                onZombieDeath?.Invoke(zombie);
                return true;
            }
        }
        #endregion
        [HarmonyPatch(typeof(Provider), nameof(Provider.accept), new Type[] { typeof(SteamPending) })]
        class ClientAcceptedPatch
        {
            internal static bool OnPreClientAcceptedInvoker(SteamPending player)
            {

                Logger.Log($"{player.playerID.characterName}");

                //player.backpackItem = 83000;

                return true;
            }
        }

        #endregion
    }
}
