using HarmonyLib;
using LogicLink.LogicV1.Plane;
using LogicLink.LogicV1.Selection;
using System.Collections.Generic;

namespace LogicLink.LogicV1.Patches;

[HarmonyPatch(typeof(LEV_GizmoHandler), "DuplicateSelectedObjects")]
public class LEV_GizmoHandler_DuplicateSelectedObjects
{
    public static void Prefix()
    {
        PlaneManager.Instance?.SelectionDuplicated();
    }
}

[HarmonyPatch(typeof(LEV_GizmoHandler), "GoIntoGMode")]
public class LEV_GizmoHandler_GoIntoGMode
{
    public static void Prefix()
    {
        OldSelectionManager selectionManager = OldSelectionManager.Instance;
        if (selectionManager == null) return;

        selectionManager.IsGrabbingOrDragging = true;
    }
}

[HarmonyPatch(typeof(LEV_GizmoHandler), "GoOutOfGMode")]
public class LEV_GizmoHandler_GoOutOfGMode
{
    public static void Prefix()
    {
        OldSelectionManager selectionManager = OldSelectionManager.Instance;
        if (selectionManager == null) return;

        selectionManager.IsGrabbingOrDragging = false;
    }
}

[HarmonyPatch(typeof(LEV_GizmoHandler), "GizmoJustGotClicked")]
public class LEV_GizmoHandler_GizmoJustGotClicked
{
    public static void Prefix()
    {
        OldSelectionManager selectionManager = OldSelectionManager.Instance;
        if (selectionManager == null) return;

        selectionManager.IsGrabbingOrDragging = true;
    }
}

[HarmonyPatch(typeof(LEV_GizmoHandler), "GizmoJustGotReleased")]
public class LEV_GizmoHandler_GizmoJustGotReleased
{
    public static void Prefix()
    {
        OldSelectionManager selectionManager = OldSelectionManager.Instance;
        if (selectionManager == null) return;

        selectionManager.IsGrabbingOrDragging = false;
    }
}
