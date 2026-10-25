using HarmonyLib;

using Il2Cpp;

using Il2CppTMPro;

using MelonLoader;

using UnityEngine;
using UnityEngine.UI;

namespace DataCenter_MoreModules.Patches;

[HarmonyPatch(typeof(ComputerShop), nameof(ComputerShop.ButtonShopScreen))]
public class AddBulkBoxToShop : BulkBoxBase
{
    // The section the bulk cards live in, kept so an unlock can refresh them without reopening the shop
    private static Transform _container;

    public static void Postfix(ComputerShop __instance)
    {
        var parent = __instance.shopItemParent;
        if (parent == null || __instance.shopItems == null)
        {
            return;
        }

        // The shop sizes its sections itself (grid -> vertical layout -> content size fitter), nothing to resize here
        var targetContainer = parent.transform.Find("HL Mods");
        if (targetContainer == null)
        {
            MelonLogger.Warning("[BulkBox] 'HL Mods' section not found in the shop, no bulk boxes added");
            return;
        }
        _container = targetContainer;

        foreach (var config in BoxConfigs)
        {
            var uniqueCardName = $"ShopCard_Bulk_{config.OriginalID}_{config.Quantity}";

            var templateItem = FindTemplate(__instance, config);
            if (templateItem == null)
            {
                continue;
            }

            var existing = targetContainer.Find(uniqueCardName);
            var card = existing != null ? existing.gameObject : CreateCard(__instance, targetContainer, uniqueCardName, config, templateItem);

            RefreshCard(card, config, templateItem);
        }
    }

    private static ShopItem FindTemplate(ComputerShop shop, BulkConfig config)
    {
        foreach (var item in shop.shopItems)
        {
            if (item?.shopItemSO != null &&
                item.shopItemSO.itemType == PlayerManager.ObjectInHand.SFPBox &&
                item.shopItemSO.itemID == config.OriginalID)
            {
                return item;
            }
        }
        return null;
    }

    private static GameObject CreateCard(ComputerShop shop, Transform container, string cardName, BulkConfig config, ShopItem templateItem)
    {
        var card = UnityEngine.Object.Instantiate(templateItem.gameObject, container);
        card.name = cardName;

        var si = card.GetComponent<ShopItem>();
        if (si != null)
        {
            UnityEngine.Object.DestroyImmediate(si);
        }

        foreach (var img in card.GetComponentsInChildren<Image>(true))
        {
            var n = img.name.ToLower();
            if (n == "bcg")
            {
                img.color = new Color(0.7f, 0.8f, 1.0f);
            }
            else if (n == "image" && templateItem.shopItemSO?.sprite != null)
            {
                img.sprite = templateItem.shopItemSO.sprite;
                img.color = Color.white;
            }
        }

        var btnExt = card.GetComponentInChildren<ButtonExtended>(true);
        if (btnExt != null)
        {
            btnExt.onClick.RemoveAllListeners();
            btnExt.interactable = true;

            Action buyAction = () =>
            {
                try
                {
                    // Look the template up again, the unit price and unlock state are read at click time
                    var template = FindTemplate(shop, config);
                    if (template == null || !CoopGate.BulkAllowed())
                    {
                        return;
                    }

                    // A locked card unlocks the module type like the single box does (spends XP, or tells the player they lack it)
                    if (!template.isUnlocked)
                    {
                        template.ButtonBuyItem();
                        return;
                    }

                    CartHelper.AddToCart(
                        shop,
                        config.OriginalID,
                        config.PriceFor(template.shopItemSO.price),
                        template.shopItemSO.itemType,
                        config.BulkName);
                    MelonLogger.Msg($"[BulkBox] Added {config.BulkName} to Cart!");
                }
                catch (Exception ex) { MelonLogger.Error($"[BulkBox] Cart Error: {ex}"); }
            };

            btnExt.onClick.AddListener(
                Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.Events.UnityAction>(buyAction));
        }

        return card;
    }

    // Called when a shop item changes its lock state, so the bulk cards follow without reopening the shop
    public static void RefreshCardsFor(ShopItem template)
    {
        if (_container == null || template.shopItemSO == null)
        {
            return;
        }

        if (template.shopItemSO.itemType != PlayerManager.ObjectInHand.SFPBox)
        {
            return;
        }

        foreach (var config in BoxConfigs)
        {
            if (config.OriginalID != template.shopItemSO.itemID)
            {
                continue;
            }

            var card = _container.Find($"ShopCard_Bulk_{config.OriginalID}_{config.Quantity}");
            if (card != null)
            {
                RefreshCard(card.gameObject, config, template);
            }
        }
    }

    // Runs on every shop open so prices follow the shop, and when the lock state of the single box changes
    private static void RefreshCard(GameObject card, BulkConfig config, ShopItem templateItem)
    {
        foreach (var txt in card.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            // The Unlock overlay has a child named "Text" as well, only the card's own labels are meant
            if (txt.transform.parent != card.transform)
            {
                continue;
            }

            var n = txt.name.ToLower();
            if (n == "textprice")
            {
                txt.text = $"{config.PriceFor(templateItem.shopItemSO.price)} $";
            }
            else if (n == "text")
            {
                txt.text = config.BulkName;
            }
        }

        // The clone's ShopItem is gone, so its lock overlay (and the "Unlock for N xp" text) has to mirror the single box
        var unlock = card.transform.Find("Unlock");
        if (unlock != null)
        {
            unlock.gameObject.SetActive(!templateItem.isUnlocked);
            var xpText = unlock.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (xpText != null && templateItem.txtXpToUnlock != null)
            {
                xpText.text = templateItem.txtXpToUnlock.text;
            }
        }
        // Hidden while a vanilla player is in the coop session, see CoopGate
        card.SetActive(CoopGate.BulkAllowed());
    }
}

[HarmonyPatch(typeof(ShopItem), nameof(ShopItem.UpdateVisualState))]
public class RefreshBulkCardsOnLockChange
{
    public static void Postfix(ShopItem __instance)
    {
        AddBulkBoxToShop.RefreshCardsFor(__instance);
    }
}
