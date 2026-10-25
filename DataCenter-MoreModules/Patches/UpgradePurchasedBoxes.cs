using HarmonyLib;

using Il2Cpp;

using MelonLoader;


namespace DataCenter_MoreModules.Patches;

// Checkout spawns one plain 5-slot box per purchased unit inside SpawnAllPurchasedItems and announces each one to the
// other players right after it spawned. The cart is read before the spawning starts and every spawned box is upgraded
// right after SpawnPhysicalItem, so the announcement already carries the bulk capacity and a save can never catch it un-upgraded.
[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.SpawnAllPurchasedItems))]
public class UpgradePurchasedBoxes : BulkBoxBase
{
    // Bulk capacities still to hand out, per sfpBoxType (== cart itemID)
    private static readonly Dictionary<int, Queue<int>> _pendingCapacities = new();

    // Spawned objects that were there before the batch or are already handled, by instance id
    private static readonly HashSet<int> _handledObjects = new();

    public static void Prefix(ComputerShop __instance)
    {
        _pendingCapacities.Clear();
        _handledObjects.Clear();

        if (__instance.cartUIItems == null)
        {
            return;
        }

        foreach (var cartItem in __instance.cartUIItems)
        {
            if (cartItem == null)
            {
                continue;
            }

            foreach (var config in BoxConfigs)
            {
                if (cartItem.itemID != config.OriginalID || cartItem.itemName != config.BulkName)
                {
                    continue;
                }

                if (!_pendingCapacities.TryGetValue(config.OriginalID, out var queue))
                {
                    queue = new Queue<int>();
                    _pendingCapacities[config.OriginalID] = queue;
                }

                for (var i = 0; i < cartItem.Quantity; i++)
                {
                    queue.Enqueue(config.Quantity);
                }

                MelonLogger.Msg($"[BulkBox] Ordered {cartItem.Quantity}x of {config.BulkName}");
            }
        }

        if (_pendingCapacities.Count == 0)
        {
            return;
        }

        foreach (var existing in __instance.spawnedItems.Values)
        {
            if (existing != null)
            {
                _ = _handledObjects.Add(existing.GetInstanceID());
            }
        }
    }

    public static void Postfix()
    {
        foreach (var pair in _pendingCapacities)
        {
            if (pair.Value.Count > 0)
            {
                MelonLogger.Warning($"[BulkBox] {pair.Value.Count} bulk box(es) of type {pair.Key} were not spawned");
            }
        }

        _pendingCapacities.Clear();
        _handledObjects.Clear();
    }

    // SpawnPhysicalItem returns the uid of the new object as a Nullable<int>, which is unreliable to read through the interop.
    // The new box is the one in the shop's spawned items that was not seen before.
    public static void UpgradeNewBoxes(ComputerShop shop)
    {
        if (_pendingCapacities.Count == 0)
        {
            return;
        }

        foreach (var spawned in shop.spawnedItems.Values)
        {
            if (spawned == null || !_handledObjects.Add(spawned.GetInstanceID()))
            {
                continue;
            }

            var box = spawned.GetComponentInChildren<SFPBox>(true);
            if (box != null)
            {
                UpgradeSpawnedBox(box);
            }
        }
    }

    // Which of the spawned boxes of a type becomes the 32 or the 64 does not matter: all are fresh, full and identical
    public static void UpgradeSpawnedBox(SFPBox box)
    {
        if (_pendingCapacities.TryGetValue(box.sfpBoxType, out var queue) && queue.Count > 0)
        {
            var capacity = queue.Dequeue();
            MoreModules.UpgradeToBulkBox(box, capacity);
            MelonLogger.Msg($"[BulkBox] Spawned box of type {box.sfpBoxType} upgraded to {capacity} slots");
        }
    }
}

[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.SpawnPhysicalItem))]
public class UpgradeBoxOnSpawn
{
    public static void Postfix(ComputerShop __instance)
    {
        UpgradePurchasedBoxes.UpgradeNewBoxes(__instance);
    }
}
