using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZeepSDK.LevelEditor;

namespace LogicLink;

public class SelectionManager
{
    public static SelectionManager Instance;


    LEV_LevelEditorCentral Central;
    Dictionary<string, SelectedParts> SelectedLogicBlocks = [];


    public SelectionManager(LEV_LevelEditorCentral central)
    {
        Central = central;
    }


    public void ClickedOnPart(Transform part)
    {
        PartType partType = GetPartType(part, out string uid);

        if (!Central.input.MultiSelect.buttonHeld)
        {
            Central.selection.DeselectAllBlocks(false, "LogicLink - ClickedOnPart");
        }

        if (!Central.input.MultiSelect.buttonHeld || !SelectedLogicBlocks.ContainsKey(uid))
        {
            SetPartSelected(uid, partType, true);
            UpdateBlockSelected(uid);
            return;
        }

        SelectedParts selectedParts = SelectedLogicBlocks[uid];
        switch (partType)
        {
            case PartType.Head:
                selectedParts.Head = !selectedParts.Head;
                break;
            case PartType.Trigger1:
                selectedParts.Trigger1 = !selectedParts.Trigger1;
                break;
            case PartType.Trigger2:
                selectedParts.Trigger2 = !selectedParts.Trigger2;
                break;
            case PartType.Unkown:
            default:
                break;
        }

        UpdateBlockSelected(uid);
    }


    public PartType GetPartType(Transform part, out string uid)
    {
        uid = null;
        Transform parent = part.parent;
        v16BallLogicBrain logicBrain = parent.GetComponentInChildren<v16BallLogicBrain>();
        if (logicBrain == null) return PartType.Unkown;

        uid = logicBrain.properties.UID;

        if (part.gameObject == logicBrain.input1selector) return PartType.Trigger1;
        else if (part.gameObject == logicBrain.input2selector) return PartType.Trigger2;
        else return PartType.Head;
    }

    public void SetPartSelected(string uid, PartType partType, bool enabled)
    {
        if (!SelectedLogicBlocks.ContainsKey(uid)) SelectedLogicBlocks[uid] = new();
        SelectedParts selectedParts = SelectedLogicBlocks[uid];

        switch (partType)
        {
            case PartType.Head:
                selectedParts.Head = enabled;
                break;
            case PartType.Trigger1:
                selectedParts.Trigger1 = enabled;
                break;
            case PartType.Trigger2:
                selectedParts.Trigger2 = enabled;
                break;
            case PartType.Unkown:
            default:
                break;
        }
    }


    public void UpdateBlockSelected(string uid)
    {
        if (!SelectedLogicBlocks.ContainsKey(uid)) return;
        SelectedParts selectedParts = SelectedLogicBlocks[uid];

        if (selectedParts.NoneSelected)
        {
            DeselectBlock(uid);
            return;
        }

        SelectBlock(uid);
    }


    public bool IsBlockSelected(string uid)
    {
        foreach (BlockProperties block in Central.selection.list)
        {
            if (block.UID == uid) return true;
        }
        return false;
    }

    public BlockProperties GetBlockProperties(string uid)
    {
        return Central.undoRedo.allBlocksDictionary[uid];
    }

    public void SelectBlock(string uid)
    {
        if (IsBlockSelected(uid)) return;
        BlockProperties block = GetBlockProperties(uid);

        List<string> selectionUIDs_before = Central.undoRedo.ConvertSelectionToStringList(Central.selection.list);
        Central.selection.AddThisBlock(block);
        List<string> selectionUIDs_after = Central.undoRedo.ConvertSelectionToStringList(Central.selection.list);
        Central.selection.RegisterManualSelectionBreakLock(selectionUIDs_before, selectionUIDs_after);
        UpdateGizmo();

        Plugin.Logger.LogMessage("Selected block!");
    }

    public void DeselectBlock(string uid)
    {
        SelectedLogicBlocks.Remove(uid);

        if (!IsBlockSelected(uid)) return;
        BlockProperties block = GetBlockProperties(uid);

        List<BlockProperties> list = Central.selection.list;
        for (int i = 0; i < list.Count; i++)
        {
            if (block == list[i])
            {
                Central.selection.RemoveBlockAt(i, false, false);
                break;
            }
        }

        UpdateGizmo();

        Plugin.Logger.LogMessage("Deselected block!");
    }

    public void UpdateGizmo()
    {
        List<BlockProperties> list = Central.selection.list;
        if (list.Count == 0) return;
        Central.gizmos.SetNewBlockGridHeight(list[^1].transform.position.y);
    }



    public void TranslateBlock(BlockProperties block, Vector3 ogTranslation)
    {
        string uid = block.UID;
        if (!SelectedLogicBlocks.ContainsKey(uid))
        {
            block.transform.Translate(ogTranslation, Space.World);
            return;
        }

        BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
        if (logicEdit == null)
        {
            Plugin.Logger.LogMessage("Could not find BlockEdit component!");
            return;
        }


        SelectedParts selectedParts = SelectedLogicBlocks[uid];
        Vector3 translation = ogTranslation;
        translation.z /= 16 * block.transform.localScale.z;
        translation.y /= 16 * block.transform.localScale.y;

        float distance1 = logicEdit.distance1;
        float height1 = logicEdit.height1;
        float distance2 = logicEdit.distance2;
        float height2 = logicEdit.height2;

        if (selectedParts.Head)
        {
            block.transform.Translate(ogTranslation, Space.World);

            if (!selectedParts.Trigger1)
            {
                distance1 -= translation.z;
                height1 -= translation.y;
            }
            if (!selectedParts.Trigger2)
            {
                distance2 -= translation.z;
                height2 -= translation.y;
            }
        }
        else
        {
            if (selectedParts.Trigger1)
            {
                distance1 += translation.z;
                height1 += translation.y;
            }
            if (selectedParts.Trigger2)
            {
                distance2 += translation.z;
                height2 += translation.y;
            }
        }

        LEV_InspectorBridge bridge = logicEdit.properties2.bridge;
        bridge.SetFloatValue(logicEdit.NUMBER_distance1, distance1);
        bridge.SetFloatValue(logicEdit.NUMBER_height1, height1);
        bridge.SetFloatValue(logicEdit.NUMBER_distance2, distance2);
        bridge.SetFloatValue(logicEdit.NUMBER_height2, height2);
        logicEdit.LogicValueChanged();
    }



    public void OnDeselectEverything()
    {
        Plugin.Logger.LogMessage("Deselected everything!");
        SelectedLogicBlocks.Clear();
    }
}
