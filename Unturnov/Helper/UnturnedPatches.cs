using HarmonyLib;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using Steamworks;
using System;
using System.Collections.Generic;
using Logger = Rocket.Core.Logging.Logger;

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
        // weight system
        //PlayerMovement: ReceivePluginGravityMultiplier ReceivePluginJumpMultiplier ReceivePluginSpeedMultiplier

        public delegate void PreDestroyBarricade(BarricadeDrop barricade, byte x, byte y, ushort plant);
        public static event PreDestroyBarricade OnPreDestroyBarricade;
        public delegate void PostGetInput(InputInfo inputInfo, ERaycastInfoUsage usage, ref bool shouldAllow);
        public static event PostGetInput OnPostGetInput;
        public delegate void PreBarricadeStorageRequest(InteractableStorage storage, ServerInvocationContext context, bool quickGrab, ref bool shouldAllow);
        public static event PreBarricadeStorageRequest OnPreBarricadeStorageRequest;
        

        public delegate void PreTryAddItemAuto(PlayerInventory inventory, Item item, ref bool autoEquipWeapon, ref bool autoEquipUseable, ref bool autoEquipClothing);
        public static event PreTryAddItemAuto OnPreTryAddItemAuto;
        public delegate void PreAttachMagazine(UseableGun gun, byte page, byte x, byte y, byte[] hash);
        public static event PreAttachMagazine OnPreAttachMagazine;
        public delegate void PostAttachMagazine(UseableGun gun);
        public static event PostAttachMagazine OnPostAttachMagazine;
        public delegate void PreEquipmentUpdateState(PlayerEquipment equipment);
        public static event PreEquipmentUpdateState OnPreEquipmentUpdateState;

        public delegate void PreInteractabilityCondition(ObjectAsset objectAsset, Player player, ref bool shouldAllow);
        public static event PreInteractabilityCondition OnPreInteractabilityCondition;
        public delegate void PreItemConditionMet(NPCItemCondition itemCondition, Player player, ref bool shouldAllow);
        public static event PreItemConditionMet OnPreItemConditionMet;

        public delegate void PreShutdownSave(ref bool shouldAllow);
        public static event PreShutdownSave OnPreShutdownSave;
        public delegate void PreDisconnectSave(CSteamID steamID, ref bool shouldAllow);
        public static event PreDisconnectSave OnPreDisconnectSave;
        
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

        public delegate void PreSendSetFlag(PlayerQuests playerQuests, ushort id, short value, ref bool shouldAllow);  
        public static event PreSendSetFlag OnSendSetFlag;
        public delegate void AnimalDeath(Animal animal);  
        public static event AnimalDeath onAnimalDeath;
        public delegate void ZombieDeath(Zombie zombie);
        public static event ZombieDeath onZombieDeath;
        #endregion
        #endregion

        #region Patches
        [HarmonyPatch(typeof(PlayerInput), nameof(PlayerInput.getInput), new Type[] { typeof(bool), typeof(ERaycastInfoUsage) })]
        class PlayerInputPatch
        {
            [HarmonyPostfix]
            internal static void OnPostGetInputInvoker(ref InputInfo __result, ERaycastInfoUsage usage)
            {
                if (__result == null)
                    return;

                bool shouldAllow = true;
                
                try
                {
                    OnPostGetInput?.Invoke(__result, usage, ref shouldAllow);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPostGetInput Patch: ");
                }
                if (!shouldAllow)
                    __result = null;
            }
        }
        [HarmonyPatch(typeof(PlayerEquipment), nameof(PlayerEquipment.updateState))]
        class EquipmentUpdateStatePatch
        {
            [HarmonyPrefix]
            internal static bool OnPreEquipmentUpdateStateInvoker(PlayerEquipment __instance)
            {
                try
                {
                    OnPreEquipmentUpdateState?.Invoke(__instance);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreEquipmentUpdateState Patch: ");
                }
                
                return true;
            }
        }

        [HarmonyPatch(typeof(BarricadeManager), nameof(BarricadeManager.destroyBarricade), new Type[] { typeof(BarricadeDrop), typeof(byte), typeof(byte), typeof(ushort) })]
        class DestroyBarricadePatch
        {
            [HarmonyPrefix]
            internal static bool OnPreDestroyBarricadeInvoker(BarricadeDrop barricade, byte x, byte y, ushort plant)
            {
                try
                {
                    OnPreDestroyBarricade?.Invoke(barricade, x, y, plant);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreDestroyBarricade Patch: ");
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(InteractableStorage), nameof(InteractableStorage.ReceiveInteractRequest))]
        class BarricadeStorageRequest
        {
            [HarmonyPrefix]
            internal static bool OnPreBarricadeStorageRequestInvoker(InteractableStorage __instance, ServerInvocationContext context, bool quickGrab)
            {
                bool shouldAllow = true;
                try
                {
                    OnPreBarricadeStorageRequest?.Invoke(__instance, context, quickGrab, ref shouldAllow);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreBarricadeStorageRequest Patch: ");
                }
                
                return shouldAllow;
            }
        }

        [HarmonyPatch(typeof(UseableGun), nameof(UseableGun.ReceiveAttachMagazine), new Type[] { typeof(byte), typeof(byte), typeof(byte), typeof(byte[])})]
        class ReceiveAttachMagazinePatch
        {
            [HarmonyPrefix]
            internal static bool OnPreAttachMagazineInvoker(UseableGun __instance, byte page, byte x, byte y, byte[] hash, out UseableGun __state)
            {
                try
                {
                    OnPreAttachMagazine?.Invoke(__instance, page, x, y, hash);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreAttachMagazine Patch: ");
                }
                __state = __instance;
                return true;
            }
            [HarmonyPostfix]
            internal static void OnPostAttachMagazineInvoker(UseableGun __state)
            {
                try
                {
                    OnPostAttachMagazine?.Invoke(__state);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPostAttachMagazine Patch: ");
                }
            }
        }
        [HarmonyPatch(typeof(PlayerInventory), nameof(PlayerInventory.tryAddItemAuto), new Type[] { typeof(Item), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        class TryAddItemAutoPatch
        {
            [HarmonyPrefix]
            internal static bool OnPreTryAddItemAutoInvoker(PlayerInventory __instance, Item item, ref bool autoEquipWeapon, ref bool autoEquipUseable, ref bool autoEquipClothing)
            {
                try
                {
                    OnPreTryAddItemAuto?.Invoke(__instance, item, ref autoEquipWeapon, ref autoEquipUseable, ref autoEquipClothing);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreTryAddItemAuto Patch: ");
                }
                
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
                try
                {
                    OnPreInteractabilityCondition?.Invoke(__instance, player, ref shouldAllow);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreInteractabilityCondition Patch: ");
                }
                
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
        
        [HarmonyPatch(typeof(SaveManager), "onServerDisconnected")]
        class DisconnectSave
        {
            [HarmonyPrefix]
            internal static bool OnPreDisconnectSaveInvoker(CSteamID steamID)
            {
                var shouldAllow = true;
                try
                {
                    OnPreDisconnectSave?.Invoke(steamID, ref shouldAllow);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreDisconnectSave Patch: ");
                }
                
                return shouldAllow;
            }
        }
        [HarmonyPatch(typeof(SaveManager), "onServerShutdown")]
        class ShutdownSave
        {
            [HarmonyPrefix]
            internal static bool OnPreShutdownSaveInvoker()
            {
                var shouldAllow = true;
                
                try
                {
                    OnPreShutdownSave?.Invoke(ref shouldAllow);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreShutdownSave Patch: ");
                }
                return shouldAllow;
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
                try
                {
                    OnPrePlayerDead?.Invoke(__instance);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPrePlayerDead Patch: ");
                }
                
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
                try
                {
                    OnPostPlayerRevive?.Invoke(__state);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPostPlayerRevive Patch: ");
                }
                
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
                try
                {
                    OnPrePlayerDraggedItem?.Invoke(__instance, page_0, x_0, y_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPrePlayerDraggedItem Patch: ");
                }
                
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
                
                try
                {
                    OnPrePlayerSwappedItem?.Invoke(__instance, page_0, x_0, y_0, rot_0, page_1, x_1, y_1, rot_1, ref shouldAllow);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPrePlayerSwappedItem Patch: ");
                }
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
                    try
                    {
                        OnPrePlayerAddItem?.Invoke((PlayerInventory)target, __instance, item, ref __result, ref shouldAllow);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e, $"Exception in OnPrePlayerAddItem Patch: ");
                    }
                    
                }
                __result = shouldAllow;
                return shouldAllow;
            }
        }
        #endregion
        #region QuestExtension
        
        [HarmonyPatch(typeof(PlayerQuests), nameof(PlayerQuests.sendSetFlag))]
        class SendSetFlag
        {
            [HarmonyPrefix]
            internal static bool OnPreSendSetFlagInvoker(PlayerQuests __instance, ushort id, short value)
            {
                bool shouldAllow = true;
                try
                {
                    OnSendSetFlag?.Invoke(__instance, id, value, ref shouldAllow);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in OnPreSendSetFlag Patch: ");
                }

                return shouldAllow;
            }
        }
        [HarmonyPatch(typeof(AnimalManager), nameof(AnimalManager.sendAnimalDead))]
        class AnimalDeathManager
        {
            [HarmonyPrefix]
            internal static bool OnPreAnimalDeathInvoker(Animal animal)
            {
                try
                {
                    onAnimalDeath?.Invoke(animal);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in onAnimalDeath Patch: ");
                }
                
                return true;
            }
        }
        [HarmonyPatch(typeof(ZombieManager), nameof(ZombieManager.sendZombieDead))]
        class ZombieDeathManager
        {
            [HarmonyPrefix]
            internal static bool OnPreZombieDeathInvoker(Zombie zombie)
            {
                try
                {
                    onZombieDeath?.Invoke(zombie);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Exception in onZombieDeath Patch: ");
                }
               
                return true;
            }
        }
        #endregion
        [HarmonyPatch(typeof(Provider), nameof(Provider.accept), new Type[] { typeof(SteamPending) })]
        class ClientAcceptedPatch
        {
            [HarmonyPrefix]
            internal static bool OnPreClientAcceptedInvoker(SteamPending player)
            {
                return true;
            }
        }

        #endregion
    }
}
