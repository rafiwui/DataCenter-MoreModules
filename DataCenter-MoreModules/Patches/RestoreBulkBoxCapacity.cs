using HarmonyLib;
using Il2Cpp;

namespace DataCenter_MoreModules.Patches;

// The save only stores a box's 0/1 slot flags, so their count is the box capacity.
// The game respawns a plain 5-slot box and LoadSFPsFromSave truncates the flags to 5,
// so the box has to be re-upgraded to the saved capacity before that runs.
[HarmonyPatch(typeof(SFPBox), nameof(SFPBox.LoadSFPsFromSave))]
public class RestoreBulkBoxCapacity
{
    private const int MaxCapacity = 128;

    public static void Prefix(SFPBox __instance)
    {
        var saved = __instance.saveIntArray;
        var slots = __instance.sfpPositions;
        if (saved == null || slots == null) return;

        if (saved.Length > slots.Length && saved.Length <= MaxCapacity)
            MoreModules.UpgradeToBulkBox(__instance, saved.Length);
    }
}
