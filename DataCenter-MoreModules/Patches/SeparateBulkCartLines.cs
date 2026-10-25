using HarmonyLib;
using Il2Cpp;

namespace DataCenter_MoreModules.Patches;

// Vanilla looks up an existing cart line by itemID and itemType only, so a normal box would be merged into a bulk line.
// While a bulk line of the same id is in the cart the lookup is redone by name.
[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.ButtonBuyShopItem))]
public class SeparateBulkCartLines
{
    public static bool Prefix(ComputerShop __instance, int itemID, int price, PlayerManager.ObjectInHand itemType, string displayName)
    {
        if (itemType != PlayerManager.ObjectInHand.SFPBox) return true;
        if (!CartHelper.CartHasBulkLine(__instance, itemID, itemType)) return true;

        CartHelper.AddToCart(__instance, itemID, price, itemType, displayName);
        return false;
    }
}
