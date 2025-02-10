using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogicLink;

[HarmonyPatch(typeof(LEV_Selection), "ClickBuilding")]
public class LEV_Selection_ClickBuilding
{
    private static bool Prefix(LEV_Selection __instance)
    {
        LEV_ClickScript clickScript = __instance.central.click;

        if (!clickScript.isHoveringBuilding) return true;
        if (clickScript.hoveredBuilding.GetComponent<BlockEdit_LogicGate>() == null) return true;


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 9999999f, clickScript.buildingLayer))
        {
            Transform hit = raycastHit.transform;
            Transform parent = hit.parent;
            if (parent == null || !parent.CompareTag("BuildingBlock")) return true;

            BlockEdit_LogicGate logicEdit = parent.GetComponent<BlockEdit_LogicGate>();
            if (logicEdit == null) return true;

            SelectionManager.Instance.ClickedOnPart(hit);

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(LEV_Selection), "TranslatePositions")]
public class LEV_Selection_TranslatePositions
{
    private static bool Prefix(LEV_Selection __instance, Vector3 translation)
    {
        if (SelectionManager.MoveMode == MoveMode.Combined) return true;

        SelectionManager selectionManager = SelectionManager.Instance;
        List<BlockProperties> list = __instance.list;
        for (int i = 0; i < list.Count; i++)
        {
            selectionManager.TranslateBlock(list[i], translation);
        }
        return false;
    }
}

[HarmonyPatch(typeof(LEV_Selection), "CalculateMiddlePivot")]
public class LEV_Selection_CalculateMiddlePivot
{
    private static bool Prefix(bool forceDefault)
    {
        if (SelectionManager.MoveMode == MoveMode.Combined) return true;

        SelectionManager.Instance.CalculateMiddlePivot(forceDefault);
        return false;
    }
}

[HarmonyPatch(typeof(LEV_Selection), "DeselectAllBlocks")]
public class LEV_Selection_DeselectAllBlocks
{
    private static void Prefix()
    {
        SelectionManager.Instance.OnDeselectEverything();
    }
}

