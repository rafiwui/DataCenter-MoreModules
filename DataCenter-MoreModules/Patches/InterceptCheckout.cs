using DataCenter_MoreModules;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace DataCenter_MoreModules.Patches;

[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.ButtonCheckOut))]
public class InterceptCheckout : BulkBoxBase
{
    public static void Prefix(ComputerShop __instance)
    {
        if (__instance.cartUIItems == null) return;

        int totalNewOrders = 0;

        foreach (var cartItem in __instance.cartUIItems)
        {
            if (cartItem == null) continue;

            foreach (var config in BoxConfigs)
            {
                if (cartItem.itemID == config.OriginalID && cartItem.itemName == config.BulkName)
                {
                    if (!PendingBoxesByName.ContainsKey(config.BulkName)) PendingBoxesByName[config.BulkName] = 0;

                    PendingBoxesByName[config.BulkName] += cartItem.Quantity;
                    totalNewOrders += cartItem.Quantity;
                    MelonLogger.Msg($"[BulkBox] Ordered {cartItem.Quantity}x of {config.BulkName}");
                }
            }
        }
        if (totalNewOrders > 0) MelonCoroutines.Start(DeliveryScannerRoutine());
    }


    private static System.Collections.IEnumerator DeliveryScannerRoutine()
    {
        bool hasPendingBoxes = true;

        while (hasPendingBoxes)
        {
            yield return new WaitForSeconds(1.5f);
           // hasPendingBoxes = false;

            var allBoxes = UnityEngine.Object.FindObjectsOfType<SFPBox>();

            foreach (var box in allBoxes)
            {
                if (box == null || box.sfpPositions == null || box.sfpPositions.Length >= 12)
                    continue; // Skip if already upgraded

                string lowerName = box.gameObject.name.ToLower();

                foreach (var config in BoxConfigs)
                {
                    if (PendingBoxesByName.ContainsKey(config.BulkName) && PendingBoxesByName[config.BulkName] > 0)
                    {
                        hasPendingBoxes = true;
                        if (lowerName.Contains(config.PrefabKeyword))
                        {
                            MoreModules.UpgradeToBulkBox(box, config.Quantity);
                            PendingBoxesByName[config.BulkName]--;
                            break;
                        }
                    }
                }
            }
            
            hasPendingBoxes = false;
            foreach (var count in PendingBoxesByName.Values)
            {
                if (count > 0) hasPendingBoxes = true;
            }
        }
    }
}