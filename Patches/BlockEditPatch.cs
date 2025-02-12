using HarmonyLib;
using LogicLink.Selection;

namespace LogicLink.Patches;

[HarmonyPatch(typeof(BlockEdit), "PropertyBreakLock")]
public class BlockEdit_PropertyBreakLock
{
    private static bool Prefix()
    {
        if (SelectionManager.Instance != null && SelectionManager.Instance.DontBreakLock) return false;
        return true;
    }
}

