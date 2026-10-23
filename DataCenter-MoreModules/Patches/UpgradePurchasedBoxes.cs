using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace DataCenter_MoreModules.Patches;

// Checkout spawns one plain 5-slot box per purchased unit inside SpawnAllPurchasedItems.
// Boxes are upgraded right after that call, in the same frame, so a save can never catch a bulk box un-upgraded.
[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.SpawnAllPurchasedItems))]
public class UpgradePurchasedBoxes : BulkBoxBase
{
    // Bulk capacities still to hand out, per sfpBoxType (== cart itemID)
    private static readonly Dictionary<int, Queue<int>> _pendingCapacities = new();
    private static readonly HashSet<int> _existingBoxIds = new();

    public static void Prefix(ComputerShop __instance)
    {
        _pendingCapacities.Clear();
        _existingBoxIds.Clear();

        if (__instance.cartUIItems == null) return;

        foreach (var cartItem in __instance.cartUIItems)
        {
            if (cartItem == null) continue;

            foreach (var config in BoxConfigs)
            {
                if (cartItem.itemID != config.OriginalID || cartItem.itemName != config.BulkName) continue;

                if (!_pendingCapacities.TryGetValue(config.OriginalID, out var queue))
                    _pendingCapacities[config.OriginalID] = queue = new Queue<int>();
                for (int i = 0; i < cartItem.Quantity; i++) queue.Enqueue(config.Quantity);
                MelonLogger.Msg($"[BulkBox] Ordered {cartItem.Quantity}x of {config.BulkName}");
            }
        }

        if (_pendingCapacities.Count == 0) return;

        foreach (var box in UnityEngine.Object.FindObjectsOfType<SFPBox>())
            _existingBoxIds.Add(box.GetInstanceID());
    }

    public static void Postfix()
    {
        if (_pendingCapacities.Count == 0) return;

        // Which of the new boxes becomes the 32 or the 64 does not matter: all are fresh, full and of the same type
        foreach (var box in UnityEngine.Object.FindObjectsOfType<SFPBox>())
        {
            if (_existingBoxIds.Contains(box.GetInstanceID())) continue;
            if (!_pendingCapacities.TryGetValue(box.sfpBoxType, out var queue) || queue.Count == 0) continue;

            MoreModules.UpgradeToBulkBox(box, queue.Dequeue());
        }

        foreach (var pair in _pendingCapacities)
            if (pair.Value.Count > 0)
                MelonLogger.Warning($"[BulkBox] {pair.Value.Count} bulk box(es) of type {pair.Key} were not found after checkout");

        _pendingCapacities.Clear();
        _existingBoxIds.Clear();
    }
}
