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

            SelectionManager.Instance.ClickedOnPart(hit, __instance.central);

            Plugin.Logger.LogMessage($"Clicked on {raycastHit.transform.parent.name} | {raycastHit.transform.name}");
            Plugin.Logger.LogMessage($"Ball: {logicEdit.ballLogicBrain.useAsBallSpawner} | Two Input: {logicEdit.ballLogicBrain.useTwoInputs}");

            return false;
        }

        return true;
    }
}

