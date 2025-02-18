using HarmonyLib;
using LogicLink.Plane;
using LogicLink.Selection;
using System.Collections.Generic;

namespace LogicLink.Patches;

[HarmonyPatch(typeof(LEV_GizmoHandler), "DuplicateSelectedObjects")]
public class LEV_GizmoHandler_DuplicateSelectedObjects
{
    public static void Prefix()
    {
        PlaneManager.Instance?.SelectionDuplicated();
    }
}
