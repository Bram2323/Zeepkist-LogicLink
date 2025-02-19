using HarmonyLib;
using LogicLink.Plane;
using LogicLink.Selection;
using System.Collections.Generic;

namespace LogicLink.Patches;

[HarmonyPatch(typeof(LEV_ToolSwitch), "DisableAllTools")]
public class LEV_ToolSwitch_DisableAllTools
{
    public static void Prefix()
    {
        SelectionManager selectionManager = SelectionManager.Instance;
        if (selectionManager == null) return;

        selectionManager.IsGrabbingOrDragging = false;
    }
}
