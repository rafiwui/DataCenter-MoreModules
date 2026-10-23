using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;

namespace DataCenter_MoreModules;

// Shows "N / Max modules" for a box, both as the crosshair label while looking at it and as a HUD line while holding it.
// The count is derived from usedPositions at display time (1 = occupied), so no state needs to be saved.
public static class ModuleCounter
{
    // Box currently under the crosshair, used to refresh the label when its contents change
    public static SFPBox Hovered;

    private static TextMeshProUGUI _hud;
    private static string _hudText;

    public static string Text(SFPBox box)
    {
        var used = box.usedPositions;
        int count = 0;
        if (used is not null)
        {
            count = used.Count<int>(x => x == 1);
        }

        string counter = $"{count} / {(used is null ? 0 : used.Length)} modules";
        // labelText is the player's own label, keep it above the counter
        return string.IsNullOrEmpty(box.labelText)
            ? counter
            : box.labelText + "\n" + counter;
    }

    public static void ShowUnderCursor(SFPBox box)
    {
        StaticUIElements.instance?.ShowTextUnderCursor(Text(box));
    }

    public static void RefreshIfHovered(SFPBox box)
    {
        if (Hovered?.Pointer == box.Pointer)
        {
            ShowUnderCursor(box);
        }
    }

    public static void UpdateHud()
    {
        var pm = PlayerManager.instance;
        SFPBox held = null;
        if (pm != null && pm.objectInHand == PlayerManager.ObjectInHand.SFPBox && pm.objectInHandGO?.Length > 0 && pm.objectInHandGO[0] != null)
        {
            held = pm.objectInHandGO[0].GetComponent<SFPBox>();
        }

        if (held is null)
        {
            if (_hud != null)
            {
                _hud.gameObject.SetActive(false);
            }
            return;
        }

        EnsureHud();
        if (_hud == null)
        {
            return;
        }

        // Only touch the TMP component when the text changed, this runs every frame
        string text = Text(held);
        if (text != _hudText)
        {
            _hud.text = text;
            _hudText = text;
        }
        _hud.gameObject.SetActive(true);
    }

    // The length label of the cable roll prefab uses the game's default UI font. Reading it from the prefab
    // works without a roll in the scene, the coin counter is the fallback.
    private static TextMeshProUGUI FindFontSource()
    {
        var manager = MainGameManager.instance;
        var spinner = manager?.GetCableSpinnerPrefab(0)?.GetComponent<CableSpinner>();
        return spinner?.txtLength is not null
            ? spinner.txtLength
            : StaticUIElements.instance.topLeft_coinTXT;
    }

    private static void EnsureHud()
    {
        // A destroyed HUD (scene change) compares equal to null, so it is simply recreated
        if (_hud != null)
        {
            return;
        }

        var ui = StaticUIElements.instance;
        if (ui == null || ui.canvasStatic == null)
        {
            return;
        }

        var go = new GameObject("MoreModules_Counter");
        go.transform.SetParent(ui.canvasStatic.transform, false);
        _hud = go.AddComponent<TextMeshProUGUI>();
        _hudText = null;

        var source = FindFontSource();
        if (source is not null)
        {
            _hud.font = source.font;
            _hud.fontSharedMaterial = source.fontSharedMaterial;
        }
        _hud.fontSize = 28;
        _hud.alignment = TextAlignmentOptions.Center;
        _hud.raycastTarget = false;
        _hud.color = Color.white;

        var rt = _hud.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.12f);
        rt.sizeDelta = new Vector2(600f, 80f);
        rt.anchoredPosition = Vector2.zero;
    }
}
