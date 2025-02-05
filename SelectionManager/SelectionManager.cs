using System;
using System.Collections.Generic;
using UnityEngine;
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
        central.inspector.Action_NothingSelected += DeselectedEverything;
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
            SelectedLogicBlocks.Remove(uid);
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
    }

    public void DeselectBlock(string uid)
    {
        if (!IsBlockSelected(uid)) return;

        UpdateGizmo();
    }

    public void UpdateGizmo()
    {
        List<BlockProperties> list = Central.selection.list;
        if (list.Count == 0) return;
        Central.gizmos.SetNewBlockGridHeight(list[^-1].transform.position.y);
    }


    public void DeselectedEverything()
    {
        SelectedLogicBlocks.Clear();
    }
}
