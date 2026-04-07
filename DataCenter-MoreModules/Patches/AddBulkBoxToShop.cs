using DataCenter_MoreModules;
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
    private static float _originalContentHeight = -1f;
    
    public static void Postfix(ComputerShop __instance)
    {
        GameObject parent = __instance.shopItemParent;
        if (parent == null || __instance.shopItems == null) return;

        Transform targetContainer = FindChildByName(parent.transform, "HL Mods");
        if (targetContainer == null) targetContainer = parent.transform;

        int cardsAddedThisTime = 0;

        foreach (var config in BoxConfigs)
        {
            string uniqueCardName = $"ShopCard_Bulk_{config.OriginalID}_{config.Quantity}";
                    
            if (targetContainer.Find(uniqueCardName) != null) continue; 

            ShopItem templateItem = null;
            foreach (var item in __instance.shopItems)
            {
                if (item?.shopItemSO != null && 
                    item.shopItemSO.itemType == PlayerManager.ObjectInHand.SFPBox && 
                    item.shopItemSO.itemID == config.OriginalID)
                {
                    templateItem = item;
                    break;
                }
            }

            if (templateItem == null) continue;

            GameObject card = UnityEngine.Object.Instantiate(templateItem.gameObject, targetContainer);
            card.name = uniqueCardName;

            var si = card.GetComponent<ShopItem>();
            if (si != null) UnityEngine.Object.DestroyImmediate(si);

            foreach (var txt in card.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string n = txt.name.ToLower();
                if (n == "textprice") txt.text = $"{config.BulkPrice} $";
                else if (n == "text") txt.text = config.BulkName;
            }

            foreach (var img in card.GetComponentsInChildren<Image>(true))
            {
                string n = img.name.ToLower();
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

                BulkConfig capturedConfig = config; 

                Action buyAction = () =>
                {
                    try
                    {
                        __instance.SpawnNewCartItem(
                            capturedConfig.OriginalID,
                            capturedConfig.BulkPrice,
                            templateItem.shopItemSO.itemType,
                            capturedConfig.BulkName,
                            new Il2CppSystem.Nullable<Color>()
                        );
                                
                        __instance.UpdateCartTotal();
                        MelonLogger.Msg($"[BulkBox] Added {capturedConfig.BulkName} to Cart!");
                    }
                    catch (Exception ex) { MelonLogger.Error($"[BulkBox] Cart Error: {ex}"); }
                };
                        
                btnExt.onClick.AddListener(
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.Events.UnityAction>(buyAction));
            }

            if (templateItem.buttonExtended != null)
            {
                templateItem.buttonExtended.onClick.RemoveAllListeners();
                ShopItem capOrig = templateItem;
                System.Action restore = () => capOrig.ButtonBuyItem();
                templateItem.buttonExtended.onClick.AddListener(
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.Events.UnityAction>(restore));
            }
            card.SetActive(true);
            cardsAddedThisTime++;
        }

        if (cardsAddedThisTime > 0)
        {
            ScrollRect sr = parent.GetComponentInParent<ScrollRect>();
            if (sr?.content != null)
            {
                RectTransform contentRT = sr.content;
                if (_originalContentHeight < 0f) _originalContentHeight = contentRT.sizeDelta.y;

                var csf = contentRT.GetComponent<ContentSizeFitter>();
                if (csf != null) csf.enabled = false;
                var parentCsf = parent.GetComponent<ContentSizeFitter>();
                if (parentCsf != null) parentCsf.enabled = false;

                float rowsAdded = Mathf.Ceil((float)cardsAddedThisTime / 4f);
                Vector2 sd = contentRT.sizeDelta;
                sd.y += (rowsAdded * 200f); 
                contentRT.sizeDelta = sd;
            }
        }
    }

    private static Transform FindChildByName(Transform root, string name)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == name) return child;
            Transform found = FindChildByName(child, name);
            if (found != null) return found;
        }
        return null;
    }
}