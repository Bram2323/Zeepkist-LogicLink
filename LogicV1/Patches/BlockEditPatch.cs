using HarmonyLib;
using LogicLink.LogicV1.Selection;

namespace LogicLink.LogicV1.Patches;

[HarmonyPatch(typeof(BlockEdit), "PropertyBreakLock")]
public class BlockEdit_PropertyBreakLock
{
    private static bool Prefix()
    {
        if (OldSelectionManager.Instance != null && OldSelectionManager.Instance.DontBreakLock) return false;
        return true;
    }
}

