using HarmonyLib;
using LogicLink.LogicV1.Selection;
using System.Collections.Generic;

namespace LogicLink.LogicV1.Patches;

[HarmonyPatch(typeof(LEV_ToolSwitch), "DisableAllTools")]
public class LEV_ToolSwitch_DisableAllTools
{
    public static void Prefix()
    {
        OldSelectionManager selectionManager = OldSelectionManager.Instance;
        if (selectionManager == null) return;

        selectionManager.IsGrabbingOrDragging = false;
    }
}
