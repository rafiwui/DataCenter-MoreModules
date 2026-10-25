using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace DataCenter_MoreModules.Patches;

// Checkout spawns one plain 5-slot box per purchased unit inside SpawnAllPurchasedItems and announces each one to the
// other players right after it spawned. The cart is read before the spawning starts and every spawned box is upgraded
// in SpawnPhysicalItem, so the announcement already carries the bulk capacity and a save can never catch it un-upgraded.
[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.SpawnAllPurchasedItems))]
public class UpgradePurchasedBoxes : BulkBoxBase
{
    // Bulk capacities still to hand out, per sfpBoxType (== cart itemID)
    private static readonly Dictionary<int, Queue<int>> _pendingCapacities = new Dictionary<int, Queue<int>>();

    public static void Prefix(ComputerShop __instance)
    {
        _pendingCapacities.Clear();

        if (__instance.cartUIItems == null)
        {
            return;
        }

        foreach (ShopCartItem cartItem in __instance.cartUIItems)
        {
            if (cartItem == null)
            {
                continue;
            }

            foreach (BulkConfig config in BoxConfigs)
            {
                if (cartItem.itemID != config.OriginalID || cartItem.itemName != config.BulkName)
                {
                    continue;
                }

                if (!_pendingCapacities.TryGetValue(config.OriginalID, out Queue<int> queue))
                {
                    queue = new Queue<int>();
                    _pendingCapacities[config.OriginalID] = queue;
                }

                for (int i = 0; i < cartItem.Quantity; i++)
                {
                    queue.Enqueue(config.Quantity);
                }

                MelonLogger.Msg($"[BulkBox] Ordered {cartItem.Quantity}x of {config.BulkName}");
            }
        }
    }

    public static void Postfix()
    {
        foreach (KeyValuePair<int, Queue<int>> pair in _pendingCapacities)
        {
            if (pair.Value.Count > 0)
            {
                MelonLogger.Warning($"[BulkBox] {pair.Value.Count} bulk box(es) of type {pair.Key} were not spawned");
            }
        }

        _pendingCapacities.Clear();
    }

    // Which of the spawned boxes of a type becomes the 32 or the 64 does not matter: all are fresh, full and identical
    public static void UpgradeSpawnedBox(SFPBox box)
    {
        if (_pendingCapacities.TryGetValue(box.sfpBoxType, out Queue<int> queue) && queue.Count > 0)
        {
            MoreModules.UpgradeToBulkBox(box, queue.Dequeue());
        }
    }
}

[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.SpawnPhysicalItem))]
public class UpgradeBoxOnSpawn
{
    public static void Postfix(ComputerShop __instance, Il2CppSystem.Nullable<int> __result, PlayerManager.ObjectInHand itemType)
    {
        if (itemType != PlayerManager.ObjectInHand.SFPBox || !__result.HasValue)
        {
            return;
        }

        if (!__instance.spawnedItems.TryGetValue(__result.Value, out GameObject spawned) || spawned == null)
        {
            return;
        }

        SFPBox box = spawned.GetComponent<SFPBox>();
        if (box != null)
        {
            UpgradePurchasedBoxes.UpgradeSpawnedBox(box);
        }
    }
}
