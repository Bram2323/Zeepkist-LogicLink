using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZeepSDK.LevelEditor;
using ZeepSDK.Messaging;

namespace LogicLink;

public class SelectionManager
{
    public static SelectionManager Instance;
    public static MoveMode MoveMode = MoveMode.Combined;


    LEV_LevelEditorCentral Central;
    Dictionary<string, SelectedParts> SelectedLogicBlocks = [];

    public bool DontBreakLock { get; private set; } = false;



    public SelectionManager(LEV_LevelEditorCentral central)
    {
        Central = central;
    }


    public static void CycleMode()
    {
        MoveMode = MoveMode switch
        {
            MoveMode.Combined => MoveMode.Strict,
            MoveMode.Strict => MoveMode.Loose,
            MoveMode.Loose => MoveMode.Combined,
            _ => MoveMode.Combined,
        };

        MessengerApi.Log($"Move Mode set to {MoveMode}");
    }

    public void SelectHeads()
    {
        BlockProperties[] list = [.. Central.selection.list];
        SelectedLogicBlocks.Clear();

        foreach (BlockProperties block in list)
        {
            BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
            if (logicEdit == null) continue;
            v16BallLogicBrain logicBrain = logicEdit.ballLogicBrain;

            SelectedParts selectedParts = new(logicBrain.useTwoInputs) { Head = true };
            SelectedLogicBlocks[block.UID] = selectedParts;
        }

        UpdateGizmo();
        CalculateMiddlePivot(false);
    }

    public void SelectTriggers()
    {
        BlockProperties[] list = [.. Central.selection.list];
        SelectedLogicBlocks.Clear();

        foreach (BlockProperties block in list)
        {
            BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
            if (logicEdit == null) continue;
            v16BallLogicBrain logicBrain = logicEdit.ballLogicBrain;

            SelectedParts selectedParts = new(logicBrain.useTwoInputs) { Trigger1 = true, Trigger2 = true };
            SelectedLogicBlocks[block.UID] = selectedParts;
        }

        UpdateGizmo();
        CalculateMiddlePivot(false);
    }

    public void SelectCombined()
    {
        BlockProperties[] list = [.. Central.selection.list];
        SelectedLogicBlocks.Clear();

        foreach (BlockProperties block in list)
        {
            BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
            if (logicEdit == null) continue;
            v16BallLogicBrain logicBrain = logicEdit.ballLogicBrain;

            SelectedParts selectedParts = new(logicBrain.useTwoInputs) { Head = true, Trigger1 = true, Trigger2 = true };
            SelectedLogicBlocks[block.UID] = selectedParts;
        }

        UpdateGizmo();
        CalculateMiddlePivot(false);
    }


    public void ClickedOnPart(Transform part)
    {
        PartType partType = GetPartType(part, out string uid, out v16BallLogicBrain logicBrain);

        if (!Central.input.MultiSelect.buttonHeld)
        {
            Central.selection.DeselectAllBlocks(false, "LogicLink - ClickedOnPart");
        }

        if (IsBlockSelected(uid) && !SelectedLogicBlocks.ContainsKey(uid))
        {
            SelectedParts parts = new(logicBrain.useTwoInputs)
            {
                Head = true,
                Trigger1 = true,
                Trigger2 = true,
            };
            SelectedLogicBlocks.Add(uid, parts);
        }

        SelectedParts selectedParts;
        if (!Central.input.MultiSelect.buttonHeld || !SelectedLogicBlocks.ContainsKey(uid))
        {
            if (!SelectedLogicBlocks.ContainsKey(uid)) SelectedLogicBlocks[uid] = new(logicBrain.useTwoInputs);
            selectedParts = SelectedLogicBlocks[uid];
            SetPartSelected(selectedParts, partType, true);
            UpdateBlockSelected(uid);
            CalculateMiddlePivot(false);
            return;
        }

        selectedParts = SelectedLogicBlocks[uid];
        if (MoveMode != MoveMode.Combined)
        {
            TogglePartSelected(selectedParts, partType);
        }
        else
        {
            SetAllPartsSelected(selectedParts, false);
        }

        UpdateBlockSelected(uid);
        CalculateMiddlePivot(false);
    }


    public PartType GetPartType(Transform part, out string uid, out v16BallLogicBrain logicBrain)
    {
        uid = null;
        logicBrain = null;

        Transform parent = part.parent;
        BlockEdit_LogicGate logicEdit = parent.GetComponent<BlockEdit_LogicGate>();
        if (logicEdit == null)
        {
            Plugin.Logger.LogWarning("BlockEdit_LogicGate not found!");
            return PartType.Unkown;
        }
        logicBrain = logicEdit.ballLogicBrain;

        uid = logicBrain.properties.UID;

        if (logicBrain.moveBallSpawnerInsteadOfTrigger)
        {
            if (part.gameObject == logicBrain.input1selector) return PartType.Head;
            else return PartType.Trigger1;
        }

        if (part.gameObject == logicBrain.input1selector) return PartType.Trigger1;
        else if (part.gameObject == logicBrain.input2selector) return PartType.Trigger2;
        else return PartType.Head;
    }

    public void SetPartSelected(SelectedParts selectedParts, PartType partType, bool selected)
    {
        switch (partType)
        {
            case PartType.Head:
                selectedParts.Head = selected;
                break;
            case PartType.Trigger1:
                selectedParts.Trigger1 = selected;
                break;
            case PartType.Trigger2:
                selectedParts.Trigger2 = selected;
                break;
            case PartType.Unkown:
            default:
                break;
        }
    }

    public void SetAllPartsSelected(SelectedParts selectedParts, bool selected)
    {
        selectedParts.Head = selected;
        selectedParts.Trigger1 = selected;
        selectedParts.Trigger2 = selected;
    }

    public void TogglePartSelected(SelectedParts selectedParts, PartType partType)
    {
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


    public void CalculateMiddlePivot(bool forceDefault)
    {
        LEV_Selection selection = Central.selection;
        List<BlockProperties> list = selection.list;

        if (list.Count == 0) return;

        if (list.Count > 1)
        {
            if (!Central.rotflip.pivotLastSelected || forceDefault)
            {
                Vector3 avgPosition = Vector3.zero;
                for (int i = 0; i < list.Count; i++)
                {
                    avgPosition += GetMiddleOfBlock(list[i]);
                }
                avgPosition /= list.Count;
                float gridXZ = Central.gizmos.gridXZ;
                float gridY = Central.gizmos.gridY;
                if (gridXZ == 0f)
                {
                    gridXZ = 0.2f;
                }
                if (gridY == 0f)
                {
                    gridY = 0.2f;
                }
                float x = Mathf.Round(avgPosition.x / gridXZ) * gridXZ;
                float y = Mathf.Round(avgPosition.y / gridY) * gridY;
                float z = Mathf.Round(avgPosition.z / gridXZ) * gridXZ;
                avgPosition = new Vector3(x, y, z);
                Central.gizmos.SetMotherPosition(avgPosition);
                return;
            }
            Central.gizmos.SetMotherPosition(Central.rotflip.GetSelectedPivot());
            return;
        }
        else
        {
            BlockProperties block = list[0];
            if (SelectedLogicBlocks.ContainsKey(block.UID))
            {
                Central.gizmos.SetMotherPosition(GetMiddleOfBlock(block));
                return;
            }

            if (!Central.rotflip.pivotLastSelected)
            {
                Central.gizmos.SetMotherPosition(block.transform.position);
                return;
            }
            Vector3 avgPosition = Vector3.zero;
            int amount = 0;
            for (int j = 0; j < block.transform.childCount; j++)
            {
                MeshCollider component = block.transform.GetChild(j).GetComponent<MeshCollider>();
                if (component != null && !component.isTrigger && !component.convex && component.gameObject.activeSelf)
                {
                    avgPosition += component.bounds.center;
                    amount++;
                }
            }
            if (amount == 0)
            {
                Central.gizmos.SetMotherPosition(block.transform.position);
                return;
            }
            Vector3 motherPosition = new(avgPosition.x / amount, avgPosition.y / amount, avgPosition.z / amount);
            Central.gizmos.SetMotherPosition(motherPosition);
            return;
        }
    }

    public Vector3 GetMiddleOfBlock(BlockProperties block)
    {
        string uid = block.UID;
        if (!SelectedLogicBlocks.ContainsKey(uid)) return block.transform.position;

        SelectedParts selectedParts = SelectedLogicBlocks[uid];
        v16BallLogicBrain logicBrain = block.GetComponentInChildren<v16BallLogicBrain>();

        if (logicBrain == null)
        {
            Plugin.Logger.LogWarning("Could not find LogicBrain!");
            return Vector3.zero;
        }

        Vector3 avgPosition = Vector3.zero;
        int amount = 0;
        if (selectedParts.Head)
        {
            avgPosition += block.transform.position;
            amount++;
        }
        if (selectedParts.Trigger1)
        {
            if (logicBrain.moveBallSpawnerInsteadOfTrigger)
            {
                avgPosition += logicBrain.ballSpawnPosition.position;
            }
            else
            {
                avgPosition += logicBrain.input1selector.transform.position;
            }
            amount++;
        }
        if (selectedParts.Trigger2)
        {
            avgPosition += logicBrain.input2selector.transform.position;
            amount++;
        }

        if (amount == 0) return Vector3.zero;
        avgPosition /= amount;
        return avgPosition;
    }


    public void TranslateBlock(BlockProperties block, Vector3 ogTranslation)
    {
        string uid = block.UID;
        if (!SelectedLogicBlocks.ContainsKey(uid) || MoveMode == MoveMode.Combined)
        {
            block.transform.Translate(ogTranslation, Space.World);
            return;
        }

        BlockEdit_LogicGate logicEdit = block.GetComponent<BlockEdit_LogicGate>();
        if (logicEdit == null)
        {
            Plugin.Logger.LogWarning("Could not find BlockEdit component!");
            return;
        }

        SelectedParts selectedParts = SelectedLogicBlocks[uid];
        if (selectedParts.AllSelected)
        {
            block.transform.Translate(ogTranslation, Space.World);
            return;
        }

        Vector3 translation = block.transform.InverseTransformVector(ogTranslation);
        if (MoveMode == MoveMode.Strict) translation.x = 0;

        float distance1 = logicEdit.distance1 * 16;
        float height1 = logicEdit.height1 * 16;
        float distance2 = logicEdit.distance2 * 16;
        float height2 = logicEdit.height2 * 16;

        if (selectedParts.Head)
        {
            Vector3 scale = block.transform.localScale;
            Vector3 localTranslation = new(translation.x * scale.x, translation.y * scale.y, translation.z * scale.z);
            block.transform.Translate(localTranslation, Space.Self);

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
            if (MoveMode == MoveMode.Loose)
            {
                Vector3 scale = block.transform.localScale;
                Vector3 looseTranslation = new(translation.x * scale.x, 0, 0);
                block.transform.Translate(looseTranslation, Space.Self);
            }

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


        if (Mathf.Approximately(translation.z, 0) && Mathf.Approximately(translation.y, 0)) return;

        LEV_InspectorBridge bridge = logicEdit.properties2.bridge;
        bridge.SetFloatValue(logicEdit.NUMBER_distance1, distance1 / 16);
        bridge.SetFloatValue(logicEdit.NUMBER_height1, height1 / 16);
        bridge.SetFloatValue(logicEdit.NUMBER_distance2, distance2 / 16);
        bridge.SetFloatValue(logicEdit.NUMBER_height2, height2 / 16);

        DontBreakLock = true;
        logicEdit.LogicValueChanged();
        DontBreakLock = false;
    }


    public void OnDeselectEverything()
    {
        Plugin.Logger.LogMessage("Deselected everything!");
        SelectedLogicBlocks.Clear();
    }
}
