using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace DataCenter_MoreModules.Patches;

// The cards are hidden when bulk boxes are unsafe, but the cart may have been filled before a vanilla player joined
[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.ButtonCheckOut))]
public class BlockBulkCheckoutWhenUnsafe
{
    public static bool Prefix(ComputerShop __instance)
    {
        if (__instance.cartUIItems == null || CoopGate.BulkAllowed())
        {
            return true;
        }

        List<ShopCartItem> bulkLines = new List<ShopCartItem>();
        foreach (ShopCartItem line in __instance.cartUIItems)
        {
            if (line != null && CartHelper.IsBulkName(line.itemName))
            {
                bulkLines.Add(line);
            }
        }

        if (bulkLines.Count == 0)
        {
            return true;
        }

        foreach (ShopCartItem line in bulkLines)
        {
            __instance.RemoveCartUIItem(line);
        }

        string message = "Bulk boxes need every player to have the More Modules mod. They were removed from the cart.";
        MelonLogger.Warning($"[BulkBox] {message}");
        StaticUIElements.instance?.AddMeesageInField(message);
        return false;
    }
}
