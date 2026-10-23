using HarmonyLib;
using Il2Cpp;

namespace DataCenter_MoreModules.Patches;

// The crosshair hits the SFPModule child for a module sitting in a box, so both types show the label.
// Vanilla hides the label on leaving only if labelText is non-empty, hence the extra hide patch.

[HarmonyPatch(typeof(SFPBox), nameof(SFPBox.InteractOnHover))]
public class CounterOnBoxHover
{
    public static void Postfix(SFPBox __instance)
    {
        // The base hover only runs with an empty hand or a held module
        var inHand = PlayerManager.instance.objectInHand;
        if (inHand != PlayerManager.ObjectInHand.None && inHand != PlayerManager.ObjectInHand.SFPModule) return;

        ModuleCounter.Hovered = __instance;
        ModuleCounter.ShowUnderCursor(__instance);
    }
}

[HarmonyPatch(typeof(SFPModule), nameof(SFPModule.InteractOnHover))]
public class CounterOnModuleHover
{
    public static void Postfix(SFPModule __instance)
    {
        if (!__instance.isInTheBox) return;

        var box = __instance.GetComponentInParent<SFPBox>();
        if (box == null) return;

        ModuleCounter.Hovered = box;
        ModuleCounter.ShowUnderCursor(box);
    }
}

[HarmonyPatch(typeof(UsableObject), nameof(UsableObject.OnHoverOver))]
public class CounterOnHoverEnd
{
    public static void Postfix(UsableObject __instance)
    {
        if (__instance.TryCast<SFPBox>() == null && __instance.TryCast<SFPModule>() == null) return;

        ModuleCounter.Hovered = null;
        StaticUIElements.instance?.HideTextUnderCursor();
    }
}

[HarmonyPatch(typeof(SFPBox), nameof(SFPBox.RemoveSFPFromBox))]
public class CounterOnRemove
{
    public static void Postfix(SFPBox __instance) => ModuleCounter.RefreshIfHovered(__instance);
}

[HarmonyPatch(typeof(SFPBox), nameof(SFPBox.ReturnSFPDirectly))]
public class CounterOnReturn
{
    public static void Postfix(SFPBox __instance) => ModuleCounter.RefreshIfHovered(__instance);
}

[HarmonyPatch(typeof(SFPBox), nameof(SFPBox.InsertSFPBackIntoBox))]
public class CounterOnInsert
{
    public static void Postfix(SFPBox __instance) => ModuleCounter.RefreshIfHovered(__instance);
}
