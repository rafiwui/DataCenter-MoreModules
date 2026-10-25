using Il2Cpp;

namespace DataCenter_MoreModules;

// Bulk and normal boxes share itemID and itemType, and the game merges cart lines by those two only.
// Everything that adds a box to the cart has to go through here so lines are matched by name as well.
public static class CartHelper
{
    public static bool IsBulkName(string name)
    {
        foreach (var config in BulkBoxBase.BoxConfigs)
            if (config.BulkName == name) return true;
        return false;
    }

    public static bool CartHasBulkLine(ComputerShop shop, int itemID, PlayerManager.ObjectInHand itemType)
    {
        if (shop.cartUIItems == null) return false;

        foreach (var line in shop.cartUIItems)
            if (line != null && line.itemID == itemID && line.itemType == itemType && IsBulkName(line.itemName))
                return true;
        return false;
    }

    public static void AddToCart(ComputerShop shop, int itemID, int price, PlayerManager.ObjectInHand itemType, string displayName)
    {
        if (shop.IsCartFullFor(itemType))
        {
            StaticUIElements.instance?.SetNotification(LocId.ContainerAlreadyFull);
            return;
        }

        ShopCartItem existing = null;
        if (shop.cartUIItems != null)
        {
            foreach (var line in shop.cartUIItems)
            {
                if (line != null && line.itemID == itemID && line.itemType == itemType && line.itemName == displayName)
                {
                    existing = line;
                    break;
                }
            }
        }

        if (existing != null)
            shop.BuyAnotherItem(itemID, price, itemType, existing);
        else
            shop.AddNewCartItem(itemID, price, itemType, displayName, new Il2CppSystem.Nullable<UnityEngine.Color>());

        shop.UpdateCartTotal();
    }
}
